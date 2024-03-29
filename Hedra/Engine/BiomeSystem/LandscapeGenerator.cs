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

            FirstPass(sampledDensity, pathMap, densityMultipliers, cutoffHeights, depth);
            SecondPass(sampledBlocks, pathMap, depth, sampledDensity, densityMultipliers, cutoffHeights, ref hasWater,
                rng, hasRiver, ref isRiverConstant);

            CopyBlocks(sampledBlocks, Blocks);
            return new ChunkDetails
            {
                HasWater = hasWater,
                IsRiverConstant = isRiverConstant
            };
        }

        private unsafe void FirstPass(float* sampledDensity, float[][] pathMap, float* densityMultipliers,
            float[] cutoffHeights, int depth)
        {
            const int border = OutOfChunkBorderSize / 2;
            var realDepth = depth + OutOfChunkBorderSize;
            var plateaus = World.WorldBuilding.GetPlateausFor(new Vector2(OffsetX, OffsetZ));
            var landforms = World.WorldBuilding.GetLandformsFor(new Vector2(OffsetX, OffsetZ));
            var hasPlateaus = plateaus.Length > 0;
            var noise3D = FillNoise(depth, Chunk.Height);
            var heights = FillHeight(depth, out _);

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

                for (var y = 0; y < Chunk.BoundsY; ++y)
                {
                    var pathDensity = CalculatePathDensity(pathHeight, y) * densityMultiplier;
                    var pathDensityModifier =
                        (float)Math.Pow(Mathf.Clamp(1f - pathDensity / BaseBiomeGenerationDesign.PathDepth, 0f, 1f), 2);
                    var heightAtPoint = calculatedHeight - pathDensity + (pathDensity > 0 ? smallFrequency : 0);
                    /* The density should also take into account rivers and what not, because of this we add the river data in the second pass*/
                    var densityAtPoint = CalculateDensity(x, y, z, noise3D) * densityMultiplier * pathDensityModifier;
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
                            y = 0;
                        }
                    }
                }

                densityMultipliers[(x + border) * realDepth + z + border] = densityMultiplier;
            }
        }

        private unsafe void SecondPass(SampledBlockWrapper sampledBlocks, float[][] pathMap, int depth,
            float* sampledDensity, float* densityMultipliers, float[] cutoffHeights, ref bool hasWater, Random rng,
            bool hasRivers, ref bool isRiverConstant)
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

                HandleStructures(position, groundworks, out var blockGroundworks, ref bonusDensity,
                    ref riverMultiplier);

                /* Water is built from the bottom up so we should not change this */
                for (var y = 0; y < Chunk.Height; ++y)
                {
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

                    GenerateBlock(sampledBlocks, ref densityForBlock, ref x, ref y, ref z, ref makeDirt, ref rng,
                        ref riverDensity, ref riverBorderDensity,
                        ref hasWater, ref pathDensity, ref cutoff, ref isRiverConstant);
                    HandleGroundworks(sampledBlocks, x, y, z, blockGroundworks);
                }

                GrassBlockPass(sampledBlocks, sampledDensity, x, z, makeDirt, depth);
            }
        }

        private static float CalculatePathDensity(float path, float y)
        {
            return Mathf.Lerp(0, path, Mathf.Clamp((y - BiomePool.SeaLevel) / 2f, 0f, 1f));
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
            var width = SampledBlocks.Width;
            for (var x = 0; x < SampledBlocks.Width; ++x)
            {
                var height = SampledBlocks.Height;
                for (var y = 0; y < SampledBlocks.Height; ++y)
                {
                    var depth = SampledBlocks.Depth;
                    for (var z = 0; z < SampledBlocks.Depth; ++z)
                    {
                        var ptr = SampledBlocks[x, y, z];
                        Blocks[x * width * height + y * depth + z].Type = ptr->Type;
                        Blocks[x * width * height + y * depth + z].Density = ptr->Density;
                        if (ptr->Type == BlockType.Water)
                            Parent.AddWaterDensity(x, y, z);
                    }
                }
            }
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

        private static unsafe void GrassBlockPass(SampledBlockWrapper Blocks, float* SampledDensity, int x, int z,
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


        private float[][] FillHeight(int width, out BlockType[][] types)
        {
            var noiseValuesWidth = width / noise2DScaleWidth + 1 + OutOfChunkBorderSize;
            Parent.Biome.Generation.BuildHeightMap(
                Noise,
                noiseValuesWidth,
                Chunk.BlockSize * noise2DScaleWidth,
                new Vector2(Parent.OffsetX - Chunk.BlockSize * noise2DScaleWidth,
                    Parent.OffsetZ - Chunk.BlockSize * noise2DScaleWidth),
                out var heights,
                out types
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
                out var densityMap,
                out var typeMap
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

        protected override void PlaceEnvironment(RegionCache Cache)
        {
            var structs = World.StructureHandler.StructureItems;
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoPlants).ToArray();
            for (var x = 0; x < Chunk.BoundsX; x++)
            for (var z = 0; z < Chunk.BoundsZ; z++)
            {
                LoopGroundworks(x, z, groundworks, out var noPlantsGroundwork);
                if (noPlantsGroundwork) continue;
                var groundY = Parent.GetHighestY(x, z);
                var waterY = Parent.GetHighestWaterY(x, z);

                void Sample(int Y, bool IsWaterPlacement)
                {
                    var samplingPosition = new Vector3(Parent.OffsetX + x * Chunk.BlockSize, Y - 1,
                        Parent.OffsetZ + z * Chunk.BlockSize);
                    var region = Cache.GetRegion(samplingPosition);
                    LoopStructures(x, z, structs, out var noWeedZone, out _, out _);
                    DoEnvironmentPlacements(samplingPosition, noWeedZone, region, IsWaterPlacement);
                }

                Sample(groundY, false);
                if (waterY > 0)
                    Sample(waterY, true);
            }
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

        private unsafe void DoEnvironmentPlacements(Vector3 Position, bool HideEnvironment, Region Biome,
            bool IsWaterPlacement)
        {
            const int size = Allocator.Kilobyte * 256;
            var mem = stackalloc byte[size];
            using (var allocator = new StackAllocator(size, mem))
            {
                var designs = Biome.Environment.Designs;
                for (var i = 0; i < designs.Length; i++)
                {
                    if (designs[i].CanBeHidden && HideEnvironment ||
                        designs[i].CanBePlacedOnWater != IsWaterPlacement) continue;
                    if (designs[i].ShouldPlace(Position, Parent))
                    {
                        var design = designs[i].GetDesign(Position, Parent, Parent.Landscape.RandomGen);
                        World.EnvironmentGenerator.GeneratePlant(allocator, Position, Biome, design);
                    }
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