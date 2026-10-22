/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:26 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Framework;
using Hedra.Numerics;

namespace Hedra.Engine.BiomeSystem
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of MountainGenerator.
    /// </summary>
    public class LandscapeGenerator : BiomeGenerator
    {
        private const float Scale = 1f;
        private const int noise2DScaleWidth = 1;
        private const int noise3DScaleWidth = 2;
        private const int noise3DScaleHeight = 8;
        private const int OutOfChunkBorderSize = 2;

        public LandscapeGenerator(Chunk Parent) : base(Parent)
        {
        }

        protected override unsafe ChunkDetails DefineBlocks(Block[] Blocks)
        {
            var rng = new Random(World.Seed + 1234123);

            const int width = (int)(Chunk.Width / Chunk.BlockSize);
            const int depth = (int)(Chunk.Width / Chunk.BlockSize);

            var sampledArray = stackalloc SampledBlock[Chunk.BoundsX * Chunk.BoundsY * Chunk.BoundsZ];
            var sampledBlocks = new SampledBlockWrapper(sampledArray, Chunk.BoundsX, Chunk.BoundsY, Chunk.BoundsZ);
            var densityMultipliers = stackalloc float[(depth + OutOfChunkBorderSize) * (width + OutOfChunkBorderSize)];
            var sampledDensity =
                stackalloc float[(depth + OutOfChunkBorderSize) * (width + OutOfChunkBorderSize) * Chunk.BoundsY];
            var cutoffHeights = new float[depth * width];
            var hasWater = false;
            var hasPath = Parent.Biome.Generation.HasPaths;
            var hasRiver = Parent.Biome.Generation.HasRivers;
            var pathMap = hasPath ? FillPath(depth, Parent.Biome.Generation) : null;
            var isRiverConstant = true;

            using (GenerationMetrics.Measure("define_first_pass"))
                FirstPass(sampledDensity, pathMap, densityMultipliers, cutoffHeights, depth);
            using (GenerationMetrics.Measure("define_second_pass"))
                SecondPass(sampledBlocks, pathMap, depth, sampledDensity, densityMultipliers, cutoffHeights, ref hasWater,
                    rng, hasRiver, ref isRiverConstant);

            using (GenerationMetrics.Measure("define_copy_blocks")) CopyBlocks(sampledBlocks, Blocks);
            if (Environment.GetEnvironmentVariable("HEDRA_VERIFY_TERRAIN") == "1")
                VerifyBlocks(Blocks, pathMap, depth, hasWater, isRiverConstant);
            return new ChunkDetails
            {
                HasWater = hasWater,
                IsRiverConstant = isRiverConstant
            };
        }

        private unsafe void FirstPass(float* sampledDensity, float[][] pathMap, float* densityMultipliers,
            float[] cutoffHeights, int depth, bool Reference = false)
        {
            const int border = OutOfChunkBorderSize / 2;
            var realDepth = depth + OutOfChunkBorderSize;
            var plateaus = World.WorldBuilding.GetPlateausFor(new Vector2(OffsetX, OffsetZ));
            var landforms = World.WorldBuilding.GetLandformsFor(new Vector2(OffsetX, OffsetZ));
            var hasPlateaus = plateaus.Length > 0;
            float[][][] noise3D;
            float[][] heights;
            using (GenerationMetrics.Measure("define_density_noise")) noise3D = FillNoise(depth, Chunk.Height);
            using (GenerationMetrics.Measure("define_height_noise")) heights = FillHeight(depth);
            var negativeX = stackalloc Vector4[Chunk.BoundsY / noise3DScaleHeight];
            var positiveX = stackalloc Vector4[Chunk.BoundsY / noise3DScaleHeight];

            for (var x = -border; x < depth + border; ++x)
            for (var z = -border; z < depth + border; ++z)
            {
                var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
                var pathHeight = x >= 0 && z >= 0 && x < depth && z < depth && pathMap != null ? pathMap[x][z] : 0;
                var landformHeight = HandleLandforms(position, landforms);
                var calculatedHeight = CalculateHeight(x, z, heights, null, out _) + landformHeight;
                var densityMultiplier = 1.0f;
                var smallFrequency = SmallFrequency(position.X, position.Y);
                var affectedByPlateau = hasPlateaus && IsAffectedByPlateau(position, plateaus);
                var foundCutoff = false;
                var abovePathDensity = CalculatePathDensity(pathHeight, BiomePool.SeaLevel + 2) * densityMultiplier;
                var abovePathModifier = (float)Math.Pow(Mathf.Clamp(1f - abovePathDensity / BaseBiomeGenerationDesign.PathDepth, 0f, 1f), 2);
                var abovePathHeight = calculatedHeight - abovePathDensity + (abovePathDensity > 0 ? smallFrequency : 0);
                var fractionZ = ((z + border) & (noise3DScaleWidth - 1)) / (float)noise3DScaleWidth;
                var zWeights = new Vector4(1 - fractionZ, 1 - fractionZ, fractionZ, fractionZ);
                if (!Reference) CacheDensityColumn(x, z, noise3D, negativeX, positiveX);

                for (var y = 0; y < Chunk.BoundsY; ++y)
                {
                    var pathDensity = abovePathDensity;
                    var pathDensityModifier = abovePathModifier;
                    var heightAtPoint = abovePathHeight;
                    if (Reference || y < BiomePool.SeaLevel + 2)
                    {
                        pathDensity = CalculatePathDensity(pathHeight, y) * densityMultiplier;
                        pathDensityModifier = (float)Math.Pow(Mathf.Clamp(1f - pathDensity / BaseBiomeGenerationDesign.PathDepth, 0f, 1f), 2);
                        heightAtPoint = calculatedHeight - pathDensity + (pathDensity > 0 ? smallFrequency : 0);
                    }
                    /* The density should also take into account rivers and what not, because of this we add the river data in the second pass*/
                    var densityAtPoint = !Reference && densityMultiplier == 0 ? 0 :
                        (Reference ? CalculateDensity(x, y, z, noise3D) :
                            InterpolateDensityColumn(y, negativeX, positiveX, zWeights)) * densityMultiplier * pathDensityModifier;
                    var calcDensity = CalculateDensityForBlock(heightAtPoint, densityAtPoint, y);
                    sampledDensity[(x + border) * realDepth * Chunk.BoundsY + y * realDepth + z + border] = calcDensity;
                    /*
                         * If we haven't found the surface and the next block has negative density then, we found it
                         * If this happens, reset the loop and start again with the correct height
                         */
                    if (!foundCutoff && calcDensity < 0)
                    {
                        foundCutoff = true;
                        if (x >= 0 && z >= 0 && x < depth && z < depth)
                            cutoffHeights[x * depth + z] = (int)Math.Round(y / .5f) * .5f;

                        if (affectedByPlateau)
                        {
                            var newFakeHeight = calcDensity * 0.35F + y - 1;
                            calculatedHeight = Mathf.Lerp(calculatedHeight + smallFrequency, newFakeHeight,
                                Mathf.Clamp(Math.Abs(calculatedHeight - y) / 8f, 0f, 1f));
                            HandlePlateaus(position, plateaus, ref calculatedHeight, ref densityMultiplier,
                                ref smallFrequency);
                            densityMultiplier = 0;
                            abovePathDensity = CalculatePathDensity(pathHeight, BiomePool.SeaLevel + 2) * densityMultiplier;
                            abovePathModifier = 1;
                            abovePathHeight = calculatedHeight - abovePathDensity;
                            y = 0;
                        }
                    }
                }

                densityMultipliers[(x + border) * realDepth + z + border] = densityMultiplier;
            }
        }

        private unsafe void SecondPass(SampledBlockWrapper sampledBlocks, float[][] pathMap, int depth,
            float* sampledDensity, float* densityMultipliers, float[] cutoffHeights, ref bool hasWater, Random rng,
            bool hasRivers, ref bool isRiverConstant, bool Reference = false)
        {
            var dirtArray = FillDirt(depth, Parent.Biome.Generation);
            float[][] riverBorderMap = null;
            var riverMap = hasRivers ? FillRiver(depth, Parent.Biome.Generation, out riverBorderMap) : null;
            var sampledDensityWidth = OutOfChunkBorderSize + depth;
            var sampledDensityOffset = OutOfChunkBorderSize / 2;
            var groundworks = World.WorldBuilding.GetGroundworksFor(new Vector2(OffsetX, OffsetZ)).ToList();

            for (var x = 0; x < depth; x++)
            for (var z = 0; z < depth; z++)
            {
                var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
                var densityMultiplier =
                    densityMultipliers[(x + sampledDensityOffset) * sampledDensityWidth + z + sampledDensityOffset];
                var makeDirt = dirtArray[x][z];
                var riverHeight = riverMap?[x][z] ?? 0;
                var pathHeight = pathMap?[x][z] ?? 0;
                var riverBorderHeight = riverBorderMap?[x][z] ?? 0;
                var pathMultiplier = densityMultiplier;
                var riverMultiplier = (1f - Math.Min(pathHeight, 1f)) * densityMultiplier;
                var smallFrequency = SmallFrequency(position.X, position.Y);
                var bonusDensity = 0f;
                var cutoff = cutoffHeights[x * depth + z];
                var riverLevel = Math.Min(BiomePool.RiverWaterLevel, cutoff);
                if (!Reference) isRiverConstant &= Math.Abs(riverLevel - BiomePool.RiverWaterLevel) < 0.005f;

                HandleStructures(position, groundworks, out var blockGroundworks, ref bonusDensity,
                    ref riverMultiplier);

                var groundworkType = BlockType.None;
                for (var g = 0; g < blockGroundworks.Length; g++)
                    if (blockGroundworks[g].Type != BlockType.None) groundworkType = blockGroundworks[g].Type;

                // Above this band, ocean/river filling and river seafloor rules
                // cannot change a block. Path and river falloffs are constant too.
                const int upperBand = (int)(BiomePool.RiverMaxHeight + 8) + 1;
                var upperPathDensity = CalculatePathDensity(pathHeight, upperBand) * pathMultiplier;
                var upperRiverDensity = (riverHeight > 0
                    ? CalculateRiverDensity(riverHeight, upperBand, ref smallFrequency, ref riverMultiplier)
                    : 0) * riverMultiplier;
                var highestNonAir = 0;

                /* Water is built from the bottom up so we should not change this */
                for (var y = 0; y < Chunk.Height; ++y)
                {
                    if (!Reference && y >= upperBand)
                    {
                        var block = sampledBlocks[x, y, z];
                        block->Density = sampledDensity[(x + sampledDensityOffset) * sampledDensityWidth * Chunk.Height
                            + y * sampledDensityWidth + z + sampledDensityOffset]
                            - upperRiverDensity * densityMultiplier + bonusDensity;
                        block->Type = block->Density > 0 ? BlockType.Stone : BlockType.Air;
                        if (block->Type != BlockType.Air)
                        {
                            HandlePathBlocks(y, block->Density, upperPathDensity, ref block->Type);
                            HandleGroundworks(sampledBlocks, x, y, z, blockGroundworks.Length != 0, groundworkType);
                            if (block->Type != BlockType.Air) highestNonAir = y;
                        }
                        continue;
                    }

                    var pathDensity = CalculatePathDensity(pathHeight, y) * pathMultiplier;
                    var riverDensity =
                        (riverHeight > 0
                            ? CalculateRiverDensity(riverHeight, y, ref smallFrequency, ref riverMultiplier)
                            : 0) * riverMultiplier;
                    var riverBorderDensity = riverBorderHeight * riverMultiplier;

                    var storedDensity =
                        sampledDensity[
                            (x + sampledDensityOffset) * sampledDensityWidth * Chunk.Height + y * sampledDensityWidth +
                            z + sampledDensityOffset];
                    var densityForBlock = storedDensity - riverDensity * densityMultiplier + bonusDensity;

                    // Air above both water levels cannot become path, ground, or water.
                    if (!Reference && densityForBlock <= 0 && y > BiomePool.SeaLevel && y >= BiomePool.RiverWaterLevel)
                    {
                        sampledBlocks[x, y, z]->Type = BlockType.Air;
                        sampledBlocks[x, y, z]->Density = densityForBlock;
                        continue;
                    }

                    GenerateBlock(sampledBlocks, ref densityForBlock, ref x, ref y, ref z, ref makeDirt, ref rng,
                        ref riverDensity, ref riverBorderDensity,
                        ref hasWater, ref pathDensity, ref cutoff, ref isRiverConstant);
                    if (Reference) HandleGroundworks(sampledBlocks, x, y, z, blockGroundworks);
                    else HandleGroundworks(sampledBlocks, x, y, z, blockGroundworks.Length != 0, groundworkType);
                    if (sampledBlocks[x, y, z]->Type != BlockType.Air) highestNonAir = y;
                }

                if (Reference) GrassBlockPassReference(sampledBlocks, sampledDensity, x, z, makeDirt, depth);
                else GrassBlockPass(sampledBlocks, sampledDensity, x, z, makeDirt, depth, highestNonAir);
            }
        }

        private static float CalculatePathDensity(float path, float y)
        {
            return Mathf.Lerp(0, path, Mathf.Clamp((y - BiomePool.SeaLevel) / 2f, 0f, 1f));
        }

        private unsafe void VerifyBlocks(Block[] Actual, float[][] PathMap, int Depth, bool HasWater,
            bool IsRiverConstant)
        {
            var reference = new SampledBlock[Actual.Length];
            var densities = new float[(Depth + OutOfChunkBorderSize) * (Depth + OutOfChunkBorderSize) * Chunk.BoundsY];
            var multipliers = new float[(Depth + OutOfChunkBorderSize) * (Depth + OutOfChunkBorderSize)];
            var cutoffs = new float[Depth * Depth];
            var hasWater = false;
            var isRiverConstant = true;
            fixed (SampledBlock* blocks = reference)
            fixed (float* density = densities, multiplier = multipliers)
            {
                // River-map sampling changes this chunk's noise seed. Replay the
                // reference from the same initial seed as its first generation.
                Noise.Seed = Seed;
                FirstPass(density, PathMap, multiplier, cutoffs, Depth, true);
                SecondPass(new SampledBlockWrapper(blocks, Chunk.BoundsX, Chunk.BoundsY, Chunk.BoundsZ), PathMap,
                    Depth, density, multiplier, cutoffs, ref hasWater, new Random(World.Seed + 1234123),
                    Parent.Biome.Generation.HasRivers, ref isRiverConstant, true);
            }
            for (var i = 0; i < Actual.Length; i++)
            {
                var block = new Block(reference[i].Type, reference[i].Density);
                if (block._bits != Actual[i]._bits)
                    throw new InvalidOperationException($"Terrain differs from reference at {OffsetX},{OffsetZ} block {i}: {block._bits} != {Actual[i]._bits}");
            }
            if (hasWater != HasWater || isRiverConstant != IsRiverConstant)
                throw new InvalidOperationException($"Terrain flags differ from reference at {OffsetX},{OffsetZ}");
            GenerationMetrics.Measure("terrain_verified").Dispose();
        }

        private static float CalculateRiverDensity(float river, float y, ref float smallFrequency,
            ref float riverMultiplier)
        {
            var falloff = river / BaseBiomeGenerationDesign.RiverDepth * riverMultiplier;
            var density = 16f + smallFrequency * 16f;
            var lerpValue = Mathf.Clamp((y - BiomePool.RiverMaxHeight) / 8f, 0f, 1f);
            density = Mathf.Lerp(density, 0, lerpValue * lerpValue);
            var bottomLerpValue = Mathf.Clamp((y - BiomePool.RiverMinHeight) / 8f, 0f, 1f);
            density = Mathf.Lerp(0, density, bottomLerpValue * bottomLerpValue);
            return density * falloff;
        }

        private bool IsAffectedByPlateau(Vector2 Position, BasePlateau[] Plateaux)
        {
            for (var i = 0; i < Plateaux.Length; i++)
                if (Plateaux[i].Density(Position) > 0)
                    return true;
            return false;
        }

        private static void HandlePlateaus(Vector2 Position, BasePlateau[] Plateaux, ref float height,
            ref float densityMultiplier, ref float smallFrequency)
        {
            for (var i = 0; i < Plateaux.Length; i++)
            {
                height = Plateaux[i].Apply(Position, height, out var final, smallFrequency);
                densityMultiplier = Math.Min(densityMultiplier, 1.0f - final);
            }
        }
        
        public static float HandleLandforms(Vector2 Position, IEnumerable<Landform> Landforms)
        {
            return Landforms.Sum(landform => landform.Apply(Position));
        }

        private static void HandleStructures(Vector2 position, List<IGroundwork> groundworks,
            out IGroundwork[] blockGroundworks, ref float height, ref float riverMultiplier)
        {
            if (groundworks.Count == 0)
            {
                blockGroundworks = Array.Empty<IGroundwork>();
                return;
            }
            blockGroundworks = groundworks.Where(G => G.Affects(position)).ToArray();
            if (blockGroundworks.Length > 0)
            {
                var groundwork = blockGroundworks[blockGroundworks.Length - 1];
                var bonusHeight =
                    Math.Min(
                        groundwork.BonusHeight,
                        groundwork.BonusHeight * groundwork.Density(position)
                    );
                height += bonusHeight;
            }

            var pathGroundwork = blockGroundworks.FirstOrDefault(P => P.IsPath);
            var groundworkDensity = pathGroundwork?.Density(position) ?? 0;
            if (pathGroundwork != null) riverMultiplier = Mathf.Lerp(riverMultiplier, 0, groundworkDensity);
        }

        private unsafe void CopyBlocks(SampledBlockWrapper SampledBlocks, Block[] Blocks)
        {
            var count = SampledBlocks.Width * SampledBlocks.Height * SampledBlocks.Depth;
            var source = SampledBlocks[0, 0, 0];
            List<CoordinateHash3D> water = null;
            for (var i = 0; i < count; i++)
            {
                var density = source[i].Density;
                var significant = Math.Min((int)Math.Abs(density * 100), 2047);
                // Match the existing Type/Density setters, including their retained
                // sign bit when a previously populated output buffer is supplied.
                Blocks[i]._bits = (ushort)(((byte)source[i].Type << 12) | (Blocks[i]._bits & 0x800)
                    | ((density < 0 ? 0 : 1) << 11) | significant);
                if (source[i].Type == BlockType.Water)
                {
                    water ??= new List<CoordinateHash3D>();
                    var x = i / (SampledBlocks.Height * SampledBlocks.Depth);
                    var y = (i / SampledBlocks.Depth) % SampledBlocks.Height;
                    var z = i % SampledBlocks.Depth;
                    water.Add(new CoordinateHash3D(x, y, z));
                }
            }
            if (water != null) Parent.AddWaterDensities(water);
        }

        private float[][] FillPath(int Width, RegionGeneration Biome)
        {
            Biome.BuildPathMap(Noise, Width, Chunk.BlockSize, new Vector2(OffsetX, OffsetZ), out var pathMap);
            return pathMap;
        }

        private float[][] FillRiver(int Width, RegionGeneration Biome, out float[][] RiverBorderMap)
        {
            Biome.BuildRiverMap(Noise, Width, Chunk.BlockSize, new Vector2(OffsetX, OffsetZ), out var riverMap,
                out RiverBorderMap);
            return riverMap;
        }

        private bool[][] FillDirt(int Width, RegionGeneration Biome)
        {
            Biome.BuildDirtMap(Noise, Width, 1, new Vector2(OffsetX, OffsetZ), out var dirtMap);
            return dirtMap;
        }

        private static unsafe void GrassBlockPass(SampledBlockWrapper Blocks, float* SampledDensity, int X, int Z,
            bool MakeDirt, int Depth, int HighestNonAir)
        {
            var foundSurface = false;
            var remaining = 0;
            for (var y = HighestNonAir; y >= 0; y--)
            {
                var block = Blocks[X, y, Z];
                if (y > 0 && y < Chunk.Height - 1)
                {
                    if (!foundSurface && block->Type != BlockType.Air && Blocks[X, y + 1, Z]->Type == BlockType.Air)
                    {
                        foundSurface = true;
                        if (CanSetGrass(SampledDensity, X, y, Z, Depth)) remaining = 8;
                    }
                    if (remaining > 0 && block->Type == BlockType.Stone)
                    {
                        block->Type = BlockType.Grass;
                        remaining--;
                    }
                }
                if (MakeDirt && block->Type == BlockType.Grass) block->Type = BlockType.Dirt;
                // Groundworks can have supplied grass farther down. Only the
                // non-dirt case can stop when the eight surface stones are done.
                if (!MakeDirt && foundSurface && remaining == 0) break;
            }
        }

        private static unsafe void GrassBlockPassReference(SampledBlockWrapper Blocks, float* SampledDensity, int x, int z,
            bool makeDirt, int depth)
        {
            var foundBlock = false;
            var counter = 0;
            for (var y = Chunk.Height - 2; y > 0; --y)
            {
                var type = Blocks[x, y, z]->Type;
                if (type != BlockType.Air && Blocks[x, y + 1, z]->Type == BlockType.Air && !foundBlock)
                {
                    foundBlock = true;
                    if (CanSetGrass(SampledDensity, x, y, z, depth))
                        counter = 8;
                }

                if (counter > 0 && type == BlockType.Stone)
                {
                    Blocks[x, y, z]->Type = BlockType.Grass;
                    counter--;
                }
            }

            for (var y = Chunk.Height - 1; y > -1; --y)
            {
                var type = Blocks[x, y, z]->Type;

                if (makeDirt && Blocks[x, y, z]->Type == BlockType.Grass) Blocks[x, y, z]->Type = BlockType.Dirt;
            }
        }

        #region Waterfall

        private static bool WaterfallPass(SampledBlock[][][] Blocks, int x, int z, Random Rng)
        {
            var isOverhang = false;
            var foundGround = false;
            var addedWater = false;
            for (var y = 0; y < Chunk.Height - 1; ++y)
            {
                var type = Blocks[x][y][z].Type;
                var upperType = Blocks[x][y + 1][z].Type;
                if (foundGround && type != BlockType.Air && upperType == BlockType.Air) isOverhang = true;
                if (!foundGround && type != BlockType.Air && upperType == BlockType.Air) foundGround = true;
                if (isOverhang && type == BlockType.Grass && upperType == BlockType.Air && Rng.Next(0, 200) == 1)
                {
                    Blocks[x][y + 1][z].Type = BlockType.Water;
                    for (var i = y; i > y - 4 && i > 0; i--) Blocks[x][i][z].Type = BlockType.Seafloor;

                    addedWater = true;
                    break;
                }
            }

            return addedWater;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateDensityForBlock(float heightAtPoint, float densityAtPoint, int Y)
        {
            return 1 - (Y - heightAtPoint) + densityAtPoint;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe bool CanSetGrass(float* sampledDensity, int x, int y, int z, int depth)
        {
            const int border = OutOfChunkBorderSize / 2;

            int IsAir(int X, int Y, int Z)
            {
                var val = sampledDensity[
                    (X + border) * (depth + OutOfChunkBorderSize) * Chunk.BoundsY + Y * (depth + OutOfChunkBorderSize) +
                    Z + border];
                return val <= 0 ? 1 : 0;
            }

            return IsAir(x + 1, y + 1, z) +
                IsAir(x, y + 1, z + 1) +
                IsAir(x - 1, y + 1, z) +
                IsAir(x, y + 1, z - 1) +
                IsAir(x + 1, y + 1, z + 1) +
                IsAir(x - 1, y + 1, z - 1) +
                IsAir(x - 1, y + 1, z + 1) +
                IsAir(x + 1, y + 1, z - 1) > 5;
        }


        private float[][] FillHeight(int width)
        {
            var noiseValuesWidth = width / noise2DScaleWidth + 1 + OutOfChunkBorderSize;
            Parent.Biome.Generation.BuildHeightMap(
                Noise,
                noiseValuesWidth,
                Chunk.BlockSize * noise2DScaleWidth,
                new Vector2(Parent.OffsetX - Chunk.BlockSize * noise2DScaleWidth,
                    Parent.OffsetZ - Chunk.BlockSize * noise2DScaleWidth),
                out var heights
            );
            return heights;
        }

        private float[][][] FillNoise(int width, int height)
        {
            /* We need to use the same scale for the noises because of FastNoiseSIMD*/
            var noiseValuesMapWidth = width / noise3DScaleWidth + 1 + OutOfChunkBorderSize;
            var noiseValuesMapHeight = height / noise3DScaleHeight + 1;
            Parent.Biome.Generation.BuildDensityMap(
                Noise,
                noiseValuesMapWidth,
                noiseValuesMapHeight,
                Chunk.BlockSize * noise3DScaleWidth,
                Chunk.BlockSize * noise3DScaleHeight,
                new Vector3(Parent.OffsetX - Chunk.BlockSize * noise3DScaleWidth, 0,
                    Parent.OffsetZ - Chunk.BlockSize * noise3DScaleWidth),
                out var densityMap
            );
            TrimTop(densityMap, noiseValuesMapWidth, noiseValuesMapHeight);
            return densityMap;
        }

        private static void TrimTop(float[][][] Density, int width, int height)
        {
            for (var x = 0; x < width; ++x)
            for (var z = 0; z < width; ++z)
            {
                Density[x][height - 1][z] = 0;
                Density[x][height - 2][z] = 0;
                Density[x][height - 3][z] = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CacheDensityColumn(int X, int Z, float[][][] Noise, Vector4* NegativeX,
            Vector4* PositiveX)
        {
            X += OutOfChunkBorderSize / 2;
            Z += OutOfChunkBorderSize / 2;
            var fractionX = (X & (noise3DScaleWidth - 1)) / (float)noise3DScaleWidth;
            var x = X / noise3DScaleWidth;
            var z = Z / noise3DScaleWidth;
            var negative = Noise[x];
            var positive = Noise[x + 1];
            for (var y = 0; y < Chunk.BoundsY / noise3DScaleHeight; y++)
            {
                var lower = negative[y];
                var upper = negative[y + 1];
                NegativeX[y] = new Vector4(lower[z], upper[z], lower[z + 1], upper[z + 1]) * (1 - fractionX);
                lower = positive[y];
                upper = positive[y + 1];
                PositiveX[y] = new Vector4(lower[z], upper[z], lower[z + 1], upper[z + 1]) * fractionX;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe float InterpolateDensityColumn(int Y, Vector4* NegativeX, Vector4* PositiveX,
            Vector4 ZWeights)
        {
            var y = Y / noise3DScaleHeight;
            var fractionY = (Y & (noise3DScaleHeight - 1)) / (float)noise3DScaleHeight;
            var yWeights = new Vector4(1 - fractionY, fractionY, 1 - fractionY, fractionY);
            return Vector4.Dot(Vector4.Multiply(NegativeX[y], yWeights), ZWeights) +
                   Vector4.Dot(Vector4.Multiply(PositiveX[y], yWeights), ZWeights);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateDensity(int x, int y, int z, float[][][] noise3D)
        {
            /* Add half the border padding since its 1 for the back border and 1 for the front one*/
            x += OutOfChunkBorderSize / 2;
            z += OutOfChunkBorderSize / 2;

            var x2 = x / noise3DScaleWidth;
            var y2 = y / noise3DScaleHeight;
            var z2 = z / noise3DScaleWidth;
            return Mathf.LinearInterpolate3D(noise3D[x2][y2][z2], noise3D[x2 + 1][y2][z2],
                noise3D[x2][y2 + 1][z2], noise3D[x2 + 1][y2 + 1][z2],
                noise3D[x2][y2][z2 + 1], noise3D[x2 + 1][y2][z2 + 1],
                noise3D[x2][y2 + 1][z2 + 1], noise3D[x2 + 1][y2 + 1][z2 + 1],
                (x & (noise3DScaleWidth-1)) / (float)noise3DScaleWidth,
                (y & (noise3DScaleHeight-1)) / (float)noise3DScaleHeight,
                (z & (noise3DScaleWidth-1)) / (float)noise3DScaleWidth
            );
        }

        private static float CalculateHeight(int x, int z, float[][] heights, BlockType[][] types, out BlockType Type)
        {
            /* Add half the border padding since its 1 for the back border and 1 for the front one*/
            x += OutOfChunkBorderSize / 2;
            z += OutOfChunkBorderSize / 2;

            var x2 = x / noise2DScaleWidth;
            var z2 = z / noise2DScaleWidth;
            Type = types != null ? types[x2][z2] : default;
            return Mathf.LinearInterpolate2D(
                heights[x2][z2], heights[x2 + 1][z2], heights[x2][z2 + 1], heights[x2 + 1][z2 + 1],
                x % noise2DScaleWidth / (float)noise2DScaleWidth,
                z % noise2DScaleWidth / (float)noise2DScaleWidth
            );
        }

        private unsafe void GenerateBlock(SampledBlockWrapper Blocks, ref float blockDensity, ref int x, ref int y,
            ref int z,
            ref bool makeDirt, ref Random rng, ref float riverHeight, ref float riverBorderHeight, ref bool hasWater,
            ref float pathDensity, ref float height, ref bool IsRiverConstant)
        {
            var blockType = BlockType.Air;

            HandleNormalBlocks(x, y, z, ref blockDensity, ref blockType, makeDirt, rng);
            HandlePathBlocks(y, blockDensity, pathDensity, ref blockType);
            hasWater |= HandleOceanBlocks(Blocks, ref x, ref y, ref z, ref blockType);
            hasWater |= HandleRiverBlocks(Blocks, ref x, ref y, ref z, ref blockType, ref riverHeight,
                ref riverBorderHeight, ref height, ref IsRiverConstant);

            Blocks[x, y, z]->Type = blockType;
            Blocks[x, y, z]->Density = blockDensity;
        }

        private static void HandlePathBlocks(in int y, in float blockDensity, in float pathDensity,
            ref BlockType blockType)
        {
            if (pathDensity / BaseBiomeGenerationDesign.PathDepth > 0.99f && blockDensity > 0)
                blockType = BlockType.Path;
        }

        private static void HandleNormalBlocks(in int x, in int y, in int z, ref float blockDensity,
            ref BlockType blockType, in bool makeDirt, in Random rng)
        {
            if (y < 2)
                blockDensity = 0.95f + rng.NextFloat() * 0.75f;

            if (blockDensity > 0)
            {
                blockType = BlockType.Stone;

                if (y < 2)
                    blockType = BlockType.Seafloor;
            }

            if (blockType == BlockType.Grass)
                if (makeDirt)
                    blockType = BlockType.Dirt;
        }

        private static unsafe bool HandleRiverBlocks(SampledBlockWrapper Blocks, ref int x, ref int y, ref int z,
            ref BlockType blockType, ref float river, ref float riverBorder, ref float height, ref bool IsRiverConstant)
        {
            var hasWater = false;
            var min = Math.Min(BiomePool.RiverWaterLevel, height);
            IsRiverConstant &= Math.Abs(min - BiomePool.RiverWaterLevel) < 0.005f;
            if (river > 0 && y < min)
                if (blockType == BlockType.Air && y > 0)
                {
                    var underType = Blocks[x, y - 1, z]->Type;
                    if (underType == BlockType.Water || underType == BlockType.Seafloor)
                    {
                        blockType = BlockType.Water;
                        hasWater = true;
                    }
                }

            if (riverBorder > 0 && y < BiomePool.RiverSeaFloorMax && y > BiomePool.RiverSeaFloorMin &&
                blockType != BlockType.Air && blockType != BlockType.Water)
                if (Blocks[x, y + 1, z]->Type == BlockType.Air)
                    blockType = BlockType.Seafloor;

            return hasWater;
        }

        private static unsafe bool HandleOceanBlocks(SampledBlockWrapper Blocks, ref int x, ref int y, ref int z,
            ref BlockType blockType)
        {
            if (y <= BiomePool.SeaLevel && blockType != BlockType.Air && y >= 1)
            {
                var underBlock = Blocks[x, y - 1, z];
                if (underBlock->Type == BlockType.Air || underBlock->Type == BlockType.Water)
                    return false;
                if (y < BiomePool.SeaLevel)
                {
                    blockType = BlockType.Seafloor;
                    for (var i = 0; i < y; i++) Blocks[x, i, z]->Type = BlockType.Seafloor;
                }
            }

            var inOceanRange = y > 0 && y <= BiomePool.SeaLevel;
            if (inOceanRange)
            {
                var underBlock = Blocks[x, y - 1, z];
                var upperBlock = Blocks[x, y + 1, z];
                var hasSolidOrWaterUnder =
                    underBlock->Type == BlockType.Seafloor || underBlock->Type == BlockType.Water;

                var hasAirAbove = blockType == BlockType.Air && upperBlock->Type == BlockType.Air;
                if (hasSolidOrWaterUnder && hasAirAbove)
                {
                    blockType = BlockType.Water;
                    return true;
                }
            }

            return false;
        }

        private unsafe void HandleGroundworks(SampledBlockWrapper Blocks, int X, int Y, int Z,
            bool HasGroundworks, BlockType GroundworkType)
        {
            if (!HasGroundworks) return;
            var block = Blocks[X, Y, Z];
            if (!IsBlockChangeable(block->Type)) return;
            if (GroundworkType != BlockType.None) block->Type = GroundworkType;
            if (block->Type == BlockType.FarmDirt && (X + OffsetX) % 2 == 0) block->Density += .25f;
        }

        private unsafe void HandleGroundworks(SampledBlockWrapper Blocks, int X, int Y, int Z,
            IGroundwork[] BlockGroundworks)
        {
            if (BlockGroundworks.Length <= 0) return;
            if (!IsBlockChangeable(Blocks[X, Y, Z]->Type)) return;

            if (BlockGroundworks.Length > 0)
            {
                var groundwork = BlockGroundworks.LastOrDefault(G => G.Type != BlockType.None);
                if (groundwork != null) Blocks[X, Y, Z]->Type = groundwork.Type;
            }

            if (Blocks[X, Y, Z]->Type == BlockType.FarmDirt)
                if ((X + OffsetX) % 2 == 0)
                    Blocks[X, Y, Z]->Density += .25f;
        }

        private bool IsBlockChangeable(BlockType Type)
        {
            return Type != BlockType.Air && Type != BlockType.Water && Type != BlockType.Seafloor;
        }

        protected override unsafe void PlaceEnvironment(RegionCache Cache)
        {
            // Plants release their temporary mesh before the next placement.
            // Reuse one scratch arena instead of zeroing 256 KB for every column.
            const int size = Allocator.Kilobyte * 256;
            var memory = stackalloc byte[size];
            using var allocator = new StackAllocator(size, memory);
            var appearances = new PlantAppearanceCache();
            var structs = World.StructureHandler.StructureItems;
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoPlants).ToArray();
            var hasWater = Parent.HasWater;
            for (var x = 0; x < Chunk.BoundsX; x++)
            for (var z = 0; z < Chunk.BoundsZ; z++)
            {
                LoopGroundworks(x, z, groundworks, out var noPlantsGroundwork);
                if (noPlantsGroundwork) continue;
                var groundY = Parent.GetHighestY(x, z);
                var waterY = hasWater ? Parent.GetHighestWaterY(x, z) : 0;

                void Sample(int Y, bool IsWaterPlacement)
                {
                    var samplingPosition = new Vector3(Parent.OffsetX + x * Chunk.BlockSize, Y - 1,
                        Parent.OffsetZ + z * Chunk.BlockSize);
                    var region = Cache.GetRegion(samplingPosition);
                    LoopStructures(x, z, structs, out var noWeedZone, out _, out _);
                    DoEnvironmentPlacements(allocator, samplingPosition, noWeedZone, region, IsWaterPlacement, appearances);
                }

                Sample(groundY, false);
                if (waterY > 0)
                    Sample(waterY, true);
            }
            appearances.Report();
        }

        protected override void DoTreeAndStructurePlacements(RegionCache Cache)
        {
            var structs = World.StructureHandler.StructureItems;
            var plateaus = World.WorldBuilding.Plateaux.Where(P => P.NoTrees).ToArray();
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoTrees).ToArray();
            for (var _x = 0; _x < Chunk.BoundsX; _x++)
            for (var _z = 0; _z < Chunk.BoundsZ; _z++)
            {
                var x = _x;
                var z = _z;
                var y = Parent.GetHighestY(x, z);
                var block = Parent.GetBlockAt(x, y, z);

                if (block.Type != BlockType.Grass) continue;
                LoopStructures(_x, _z, structs, out _, out var noTreesZone, out _);
                LoopPlateaus(_x, _z, plateaus, out var noTreesPlateau);
                LoopGroundworks(_x, _z, groundworks, out var noTreesGroundwork);

                if (World.Seed == World.MenuSeed)
                {
                    //Is menu, force a platform
                    if ((MenuBackground.DefaultPosition.Xz() - new Vector2(
                        Parent.OffsetX + x * Chunk.BlockSize,
                        Parent.OffsetZ + z * Chunk.BlockSize)).LengthSquared() < 32 * 32) continue;
                    if ((MenuBackground.CreatorPosition.Xz() - new Vector2(
                        Parent.OffsetX + x * Chunk.BlockSize,
                        Parent.OffsetZ + z * Chunk.BlockSize)).LengthSquared() < 32 * 32) continue;
                    if ((MenuBackground.CampfirePosition.Xz() - new Vector2(
                        Parent.OffsetX + x * Chunk.BlockSize,
                        Parent.OffsetZ + z * Chunk.BlockSize)).LengthSquared() < 48 * 48) continue;
                }

                if (noTreesZone || noTreesPlateau || noTreesGroundwork) continue;

                var realPosition = new Vector3(Parent.OffsetX + _x * Chunk.BlockSize, y - 1,
                    Parent.OffsetZ + _z * Chunk.BlockSize);

                var region = Cache.GetRegion(realPosition);
                var noise = World.GetNoise(realPosition.X * 0.005f, realPosition.Z * 0.005f);
                var placementObject = World.TreeGenerator.CanGenerateTree(realPosition, region);
                if (!placementObject.Placed) continue;
                World.TreeGenerator.GenerateTree(placementObject, region, region.Trees.GetDesign((int)(noise * 10000)));
            }
        }

        private void LoopPlateaus(int X, int Z, BasePlateau[] RoundedPlateaux, out bool Result)
        {
            Result = false;
            for (var i = 0; i < RoundedPlateaux.Length; i++)
                if (RoundedPlateaux[i]
                    .Collides(new Vector2(X * Chunk.BlockSize + OffsetX, Z * Chunk.BlockSize + OffsetZ)))
                {
                    Result = true;
                    return;
                }
        }

        private void LoopGroundworks(int X, int Z, IGroundwork[] Groundworks, out bool Result)
        {
            Result = false;
            for (var i = 0; i < Groundworks.Length; i++)
                if (Groundworks[i].Affects(new Vector2(X * Chunk.BlockSize + OffsetX, Z * Chunk.BlockSize + OffsetZ)))
                {
                    Result = true;
                    return;
                }
        }

        private void DoEnvironmentPlacements(IAllocator Allocator, Vector3 Position, bool HideEnvironment, Region Biome,
            bool IsWaterPlacement, PlantAppearanceCache Appearances)
        {
            var designs = Biome.Environment.Designs;
            for (var i = 0; i < designs.Length; i++)
            {
                if (designs[i].CanBeHidden && HideEnvironment ||
                    designs[i].CanBePlacedOnWater != IsWaterPlacement) continue;
                if (designs[i].ShouldPlace(Position, Parent))
                {
                    var design = designs[i].GetDesign(Position, Parent, Parent.Landscape.RandomGen);
                    World.EnvironmentGenerator.GeneratePlant(Allocator, Position, Biome, design, default, Appearances);
                }
            }
        }

        private void LoopStructures(int X, int Z, CollidableStructure[] Structs,
            out bool NoGrassZone, out bool NoTreesZone, out bool InMerchant)
        {
            NoGrassZone = false;
            NoTreesZone = false;
            InMerchant = false;
            foreach (var structPosition in Structs)
            {
                var possiblePosition = new Vector3(Parent.OffsetX + X * Chunk.BlockSize, 0,
                    Parent.OffsetZ + Z * Chunk.BlockSize);

                if (structPosition.Design is GiantTreeDesign)
                {
                    var radius = structPosition.Mountain.Radius;
                    if ((structPosition.Mountain.Position - possiblePosition.Xz()).LengthSquared() <
                        radius * .3f * radius * .3f)
                        NoGrassZone = true;

                    if (!NoTreesZone && (structPosition.Mountain.Position - possiblePosition.Xz()).LengthSquared() <
                        radius * 0.5f * radius * 0.5f)
                        NoTreesZone = true;
                }

                if (structPosition.Design is TravellingMerchantDesign)
                {
                    var radius = structPosition.Mountain.Radius;
                    if ((structPosition.Mountain.Position - possiblePosition.Xz()).LengthSquared() <
                        radius * .5f * radius * .5f)
                        InMerchant = true;
                }
            }
        }
    }
}
