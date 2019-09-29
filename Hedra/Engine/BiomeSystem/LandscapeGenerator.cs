/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:26 p.m.
 *
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using BulletSharp;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using OpenTK;
using Chunk = Hedra.Engine.Generation.ChunkSystem.Chunk;

namespace Hedra.Engine.BiomeSystem
{
    /// <inheritdoc />
    /// <summary>
    /// Description of MountainGenerator.
    /// </summary>
    public class LandscapeGenerator : BiomeGenerator
    {
        private const float Narrow = 0.42f;
        private const float Border = 0.02f;
        private const float Scale = 1f;
        private const float RiverDepth = 8;
        private const float RiverMult = 165f;
        private const float PathDepth = 2.25f;
        const int noise2DScaleWidth = 1;
        const int noise3DScaleWidth = 2;
        const int noise3DScaleHeight = 8;
        const int OutOfChunkBorderSize = 2;

        public LandscapeGenerator(Chunk Parent) : base(Parent)
        {
        }

        protected override ChunkDetails DefineBlocks(Block[] Blocks)
        {
            var rng = new Random(World.Seed + 1234123);
            
            var width = (int) (Chunk.Width / Chunk.BlockSize);
            var depth = (int) (Chunk.Width / Chunk.BlockSize);

            var noise3D = FillNoise(width, Chunk.Height);
            var heights = FillHeight(width, out var types);
            var dirtArray = FillDirt(width, Parent.Biome.Generation);
            var riverMap = FillRiver(width, Parent.Biome.Generation, out var riverBorderMap);
            var sampledBlocks = CreateSampledBlocks(width, Chunk.Height);

            var plateaus = World.WorldBuilding.GetPlateausFor(new Vector2(OffsetX, OffsetZ));
            var groundworks = World.WorldBuilding.GetGroundworksFor(new Vector2(OffsetX, OffsetZ)).ToList();

            var hasPath = 0;//biomeGen.HasPaths ? 1f : 0f;
            var hasWater = false;

            for (var x = 0; x < width; x++)
            {

                for (var z = 0; z < depth; z++)
                {
                    var makeDirt = dirtArray[x][z];
                    var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
                    var riverHeight = riverMap[x][z];
                    var riverBorderHeight = riverBorderMap[x][z];
                    var height = CalculateHeight(x, z, heights, types, out var type) - riverMap[x][z];

                    var densityClampHeight = Chunk.Height - 3f;
                    HandleStructures(position, groundworks, out var blockGroundworks, ref height);
                    HandlePlateaus(x, z, position, plateaus, ref densityClampHeight);

                    /* Water is built from the buttom up so we should not change this */
                    for (var y = 0; y < Chunk.Height; ++y)
                    {
                        var density = CalculateDensity(x,y,z, noise3D);
                        if (y >= densityClampHeight) density = 0;
                        
                        this.GenerateBlock(sampledBlocks, ref density, ref type, ref x, ref y, ref z, ref height, ref makeDirt, ref rng, ref riverHeight, ref riverBorderHeight, ref hasWater);
                                    
                        this.HandleGroundworks(sampledBlocks, x, y, z, blockGroundworks);
                    }
                    GrassBlockPass(sampledBlocks, noise3D, heights, x, z, makeDirt, width, depth);
                    hasWater |= riverHeight > 0.1f;
                }
            }

            CopyBlocks(sampledBlocks, Blocks);
            return new ChunkDetails
            {
                HasWater = hasWater
            };
        }

        private void HandlePlateaus(int X, int Z, Vector2 Position, BasePlateau[] Plateaux, ref float densityClampHeight)
        {
            var smallFrequency = SmallFrequency(Position.X, Position.Y);
            for (var i = 0; i < Plateaux.Length; i++)
            {
                var point = new Vector2(OffsetX + X * Chunk.BlockSize, OffsetZ + Z * Chunk.BlockSize);
                densityClampHeight = Plateaux[i].Apply(point, densityClampHeight, out var final, smallFrequency);
            }
        }
        
        private void HandleStructures(Vector2 position, List<IGroundwork> groundworks, out IGroundwork[] blockGroundworks, ref float height)
        {
            blockGroundworks = groundworks.Where(G => G.Affects(position)).ToArray();

            var pathGroundwork = blockGroundworks.FirstOrDefault(P => P.IsPath);
            var groundworkDensity = pathGroundwork?.Density(position) ?? 0;

            if (blockGroundworks.Length > 0)
            {
                var groundwork = blockGroundworks[blockGroundworks.Length - 1];
                var bonusHeight =
                    Math.Min(
                        groundwork.BonusHeight,
                        groundwork.BonusHeight * groundwork.Density(position) * groundwork.DensityMultiplier
                    );
                height += bonusHeight;
            }
        }

        private static void CopyBlocks(SampledBlock[][][] SampledBlocks, Block[] Blocks)
        {
            var width = SampledBlocks.Length;
            for (var x = 0; x < SampledBlocks.Length; ++x)
            {
                var height = SampledBlocks[x].Length;
                for (var y = 0; y < SampledBlocks[x].Length; ++y)
                {
                    var depth = SampledBlocks[x][y].Length;
                    for (var z = 0; z < SampledBlocks[x][y].Length; ++z)
                    {
                        Blocks[x * width * height + y * depth + z].Type = SampledBlocks[x][y][z].Type;
                        Blocks[x * width * height + y * depth + z].Density = SampledBlocks[x][y][z].Density;
                    }
                }
            }
        }

        private static SampledBlock[][][] CreateSampledBlocks(int Width, int Height)
        {
            var sampled = new SampledBlock[Width][][];
            for (var x = 0; x < sampled.Length; ++x)
            {
                sampled[x] = new SampledBlock[Height][];
                for (var y = 0; y < sampled[x].Length; ++y)
                {
                    sampled[x][y] = new SampledBlock[Width];
                }
            }
            return sampled;
        }

        private float[][] FillRiver(int Width, RegionGeneration Biome, out float[][] RiverBorderMap)
        {
            Biome.BuildRiverMap(Noise, Width, Chunk.BlockSize, new Vector2(OffsetX, OffsetZ), out var riverMap, out RiverBorderMap);
            return riverMap;
        }

        private bool[][] FillDirt(int width, RegionGeneration Biome)
        {
            var scale = 1;
            var size = width / scale;
            var dirtArray = new bool[width][];
            var noise = Noise.GetSimplexSetWithFrequency(new Vector2(OffsetX, OffsetZ), new Vector2(size, size),
                new Vector2(Chunk.BlockSize, Chunk.BlockSize), 0.005f);

            var index = 0;
            for (var x = 0; x < dirtArray.Length; x++)
            {
                dirtArray[x] = new bool[width];
                for (var y = 0; y < dirtArray[x].Length; y++)
                {
                    dirtArray[x][y] = noise[index++] - 0.3f > 0;
                }
            }
            
            return dirtArray;  
        }

        private static void GrassBlockPass(SampledBlock[][][] Blocks, float[][][] Density, float[][] HeightMap, int x, int z, bool makeDirt, int width, int depth)
        {
            var foundBlock = false;
            var counter = 0;
            for (var y = Chunk.Height - 2; y > 0; --y)
            {
                var type = Blocks[x][y][z].Type;
                if (type != BlockType.Air && Blocks[x][y+1][z].Type == BlockType.Air && !foundBlock)
                {
                    foundBlock = true;
                    if(CanSetGrass(Density, HeightMap, x, y, z, width, depth))
                        counter = 8;
                }

                if (counter > 0 && type == BlockType.Stone)
                {
                    Blocks[x][y][z].Type = BlockType.Grass;
                    counter--;
                }
            }
            for (var y = Chunk.Height - 1; y > -1; --y)
            {
                var type = Blocks[x][y][z].Type;
                
                if (makeDirt && Blocks[x][y][z].Type == BlockType.Grass)
                {
                    Blocks[x][y][z].Type = BlockType.Dirt;
                }
            }
        }
        #region Waterfall
        private static bool WaterfallPass(SampledBlock[][][] Blocks, int x, int z, Random Rng)
        {
            var isOverhang = false;
            var foundGround = false;
            var addedWater = false;
            for (var y = 0; y < Chunk.Height-1; ++y)
            {
                var type = Blocks[x][y][z].Type;
                var upperType = Blocks[x][y + 1][z].Type;
                if (foundGround && type != BlockType.Air && upperType == BlockType.Air)
                {
                    isOverhang = true;
                }
                if (!foundGround && type != BlockType.Air && upperType == BlockType.Air)
                {
                    foundGround = true;
                }
                if (isOverhang && type == BlockType.Grass && upperType == BlockType.Air && Rng.Next(0, 200) == 1)
                {
                    Blocks[x][y+1][z].Type = BlockType.Water;
                    for (var i = y; i > y - 4 && i > 0; i--)
                    {
                        Blocks[x][i][z].Type = BlockType.Seafloor;
                    }

                    addedWater = true;
                    break;
                }
            }
            return addedWater;
        }
        #endregion
        
        [MethodImpl(256)]
        private static float CalculateDensityForBlock(float heightAtPoint, float densityAtPoint, int Y)
        {
            return 1 - (Y - heightAtPoint) + densityAtPoint;
        }

        private static bool CanSetGrass(float[][][] Density, float[][] HeightMap, int x, int y, int z, int width, int depth)
        {
            int IsGood(int X, int Y, int Z)
            {
                var heightAtPoint = CalculateHeight(X, Z, HeightMap, null, out _);
                var densityAtPoint = CalculateDensity(X, Y, Z, Density);
                var valueAtPoint = CalculateDensityForBlock(heightAtPoint, densityAtPoint, Y);
                return valueAtPoint <= 0 ? 1 : 0;
            }

            return IsGood(x + 1, y + 1, z) +
                IsGood(x, y + 1, z + 1) +
                IsGood(x - 1, y + 1, z) +
                IsGood(x, y + 1, z - 1) +
                IsGood(x + 1, y + 1, z + 1) +
                IsGood(x - 1, y + 1, z - 1) +
                IsGood(x - 1, y + 1, z + 1) +
                IsGood(x + 1, y + 1, z - 1) > 5;
        }


        private float[][] FillHeight(int width, out BlockType[][] types)
        {
            var noiseValuesWidth = (width / noise2DScaleWidth + 1) + OutOfChunkBorderSize;
            Parent.Biome.Generation.BuildHeightMap(
                Noise,
                noiseValuesWidth,
                Chunk.BlockSize * noise2DScaleWidth,
                new Vector2(Parent.OffsetX - Chunk.BlockSize * noise2DScaleWidth, Parent.OffsetZ - Chunk.BlockSize * noise2DScaleWidth),
                out var heights,
                out types
            );
            return heights;
        }
        
        private float[][][] FillNoise(int width, int height)
        {
            /* We need to use the same scale for the noises because of FastNoiseSIMD*/
            var noiseValuesMapWidth = (width / noise3DScaleWidth + 1) + OutOfChunkBorderSize;
            var noiseValuesMapHeight = (height / noise3DScaleHeight + 1);
            Parent.Biome.Generation.BuildDensityMap(
                Noise,
                noiseValuesMapWidth,
                noiseValuesMapHeight,
                Chunk.BlockSize * noise3DScaleWidth,
                Chunk.BlockSize * noise3DScaleHeight,
                new Vector3(Parent.OffsetX - Chunk.BlockSize * noise3DScaleWidth, 0, Parent.OffsetZ - Chunk.BlockSize * noise3DScaleWidth),
                out var densityMap,
                out var typeMap
            );
            TrimTop(densityMap, noiseValuesMapWidth, noiseValuesMapHeight);
            return densityMap;
        }

        private static void TrimTop(float[][][] Density, int width, int height)
        {
            for (var x = 0; x < width; ++x)
            {
                for (var z = 0; z < width; ++z)
                {
                    Density[x][height - 1][z] = 0;
                    Density[x][height - 2][z] = 0;
                    Density[x][height - 3][z] = 0;
                }
            }
        }

        private static float CalculateDensity(int x, int y, int z, float[][][] noise3D)
        {
            /* Add half the border padding since its 1 for the back border and 1 for the front one*/
            x += OutOfChunkBorderSize / 2;
            z += OutOfChunkBorderSize / 2;
            
            int x2 = (x / noise3DScaleWidth);
            int y2 = (y / noise3DScaleHeight);
            int z2 = (z / noise3DScaleWidth);
            return Mathf.LinearInterpolate3D(noise3D[x2][y2][z2], noise3D[x2 + 1][y2][z2],
                noise3D[x2][y2 + 1][z2], noise3D[x2 + 1][y2 + 1][z2],
                noise3D[x2][y2][z2 + 1], noise3D[x2 + 1][y2][z2 + 1],
                noise3D[x2][y2 + 1][z2 + 1], noise3D[x2 + 1][y2 + 1][z2 + 1],
                (x % noise3DScaleWidth) / (float) noise3DScaleWidth,
                (y % noise3DScaleHeight) / (float) noise3DScaleHeight,
                (z % noise3DScaleWidth) / (float) noise3DScaleWidth
            );
        }

        private static float CalculateHeight(int x, int z, float[][] heights, BlockType[][] types, out BlockType Type)
        {
            /* Add half the border padding since its 1 for the back border and 1 for the front one*/
            x += OutOfChunkBorderSize / 2;
            z += OutOfChunkBorderSize / 2;
            
            int x2 = (x / noise2DScaleWidth);
            int z2 = (z / noise2DScaleWidth);
            Type = types != null ? types[x2][z2] : default(BlockType);
            return Mathf.LinearInterpolate2D(
                heights[x2][z2], heights[x2 + 1][z2], heights[x2][z2 + 1], heights[x2 + 1][z2 + 1],
                (x % noise2DScaleWidth) / (float) noise2DScaleWidth,
                (z % noise2DScaleWidth) / (float) noise2DScaleWidth
            );
        }
        
        private void GenerateBlock(SampledBlock[][][] Blocks, ref float density, ref BlockType Type, ref int x, ref int y, ref int z, ref float height,
            ref bool makeDirt, ref Random rng, ref float riverHeight, ref float riverBorderHeight, ref bool hasWater)
        {
            var currentBlock = Blocks[x][y][z];
            var blockType = BlockType.Air;
            var blockDensity = CalculateDensityForBlock(height, density, y);

            HandleNormalBlocks(Blocks, ref x, ref y, ref z, ref blockDensity, ref blockType, ref makeDirt, ref rng);
            hasWater |= HandleRiverBlocks(Blocks, ref x, ref y, ref z, ref blockType, ref riverHeight, ref riverBorderHeight, ref height);
            hasWater |= HandleOceanBlocks(Blocks, ref x, ref y, ref z, ref blockType);

            currentBlock.Type = blockType;
            currentBlock.Density = blockDensity;
            Blocks[x][y][z] = currentBlock;
        }

        private void HandleNormalBlocks(SampledBlock[][][] Blocks, ref int x, ref int y, ref int z, ref float blockDensity, ref BlockType blockType, ref bool makeDirt, ref Random rng)
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
            {
                if (makeDirt) blockType = BlockType.Dirt;
            }
        }

        private static bool HandleRiverBlocks(SampledBlock[][][] Blocks, ref int x, ref int y, ref int z, ref BlockType blockType, ref float river, ref float riverBorder, ref float height)
        {
            var hasWater = false;
            if (riverBorder > 0 || river > 0)
            {
                var originalHeight = height + river;
                if (river > 0 && y < originalHeight)
                {
                    if (blockType == BlockType.Air && y > 0)
                    {
                        var underType = Blocks[x][y - 1][z].Type;
                        if (underType == BlockType.Water || underType == BlockType.Seafloor)
                        {
                            blockType = BlockType.Water;
                            hasWater = true;
                        }
                    }
                }
                if (riverBorder > 0 && blockType != BlockType.Air && blockType != BlockType.Water)
                {
                    blockType = BlockType.Seafloor;
                }
            }

            return hasWater;
        }

        private static bool HandleOceanBlocks(SampledBlock[][][] Blocks, ref int x, ref int y, ref int z, ref BlockType blockType)
        {
            
            if (y <= BiomePool.SeaLevel && blockType != BlockType.Air && y >= 1)
            {
                var underBlock = Blocks[x][y - 1][z];
                if (underBlock.Type == BlockType.Air || underBlock.Type == BlockType.Water)
                    return false;
                if (y < BiomePool.SeaLevel)
                {
                    blockType = BlockType.Seafloor;
                    for (var i = 0; i < y; i++)
                    {
                        Blocks[x][i][z].Type = BlockType.Seafloor;
                    }
                }
            }
            
            var inOceanRange = y > 0 && y <= BiomePool.SeaLevel;
            if (inOceanRange)
            {            
                var underBlock = Blocks[x][y - 1][z];
                var upperBlock = Blocks[x][y +1][z];
                var hasSolidOrWaterUnder = (underBlock.Type == BlockType.Seafloor || underBlock.Type == BlockType.Water);

                var hasAirAbove = blockType == BlockType.Air && upperBlock.Type == BlockType.Air;
                if (hasSolidOrWaterUnder && hasAirAbove)
                {
                    blockType = BlockType.Water;
                    return true;
                }
            }
            return false;
        }

        private void HandleGroundworks(SampledBlock[][][] Blocks, int X, int Y, int Z, IGroundwork[] BlockGroundworks)
        {
            if (BlockGroundworks.Length <= 0) return;
            if (!this.IsBlockChangeable(Blocks[X][Y][Z].Type)) return;
            
            if (BlockGroundworks.Length > 0)
            {
                var groundwork = BlockGroundworks.LastOrDefault(G => G.Type != BlockType.None);
                if (groundwork != null)
                {
                    Blocks[X][Y][Z].Type = groundwork.Type;
                }
            }

            if (Blocks[X][Y][Z].Type == BlockType.FarmDirt)
            {
                if ((X + OffsetX) % 2 == 0) Blocks[X][Y][Z].Density += new Half(.25f);
            }
        }
        
        public static float River(Vector2 Position, float Border = 0)
        {
            return (float) Math.Max(0,
                       0.5 - Math.Abs(
                           World.GetNoise(Position.X * 0.0011f,
                               Position.Y * 0.0011f) - 0.2) - Narrow +
                       Border) * Scale;
        }

        private bool IsBlockChangeable(BlockType Type)
        {
            return Type != BlockType.Air && Type != BlockType.Water && Type != BlockType.Seafloor;
        }

        protected override void PlaceEnvironment(RegionCache Cache)
        {
            return;
            var structs = World.StructureHandler.StructureItems;
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoPlants).ToArray();
            for (var x = 0; x < Chunk.BoundsX; x++)
            {
                for (var z = 0; z < Chunk.BoundsZ; z++)
                {
                    this.LoopGroundworks(x, z, groundworks, out var noPlantsGroundwork);
                    if(noPlantsGroundwork) continue;
                    var y = Parent.GetHighestY(x, z);
                    if(y < BiomePool.SeaLevel - Chunk.BlockSize) continue;
                    var samplingPosition = new Vector3(Parent.OffsetX + x * Chunk.BlockSize, y-1, Parent.OffsetZ + z * Chunk.BlockSize);
                    
                    
                    var region = Cache.GetRegion(samplingPosition);
                    this.LoopStructures(x, z, structs, out var noWeedZone, out _, out _);
                    this.DoEnvironmentPlacements(samplingPosition, noWeedZone, region);
                }
            }
        }

        protected override void DoTreeAndStructurePlacements(RegionCache Cache)
        {
            return;
            var structs = World.StructureHandler.StructureItems;
            var plateaus = World.WorldBuilding.Plateaux.Where(P => P.NoTrees).ToArray();
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoTrees).ToArray();
            for (var _x = 0; _x < Chunk.BoundsX; _x++)
            {
                for (var _z = 0; _z < Chunk.BoundsZ; _z++)
                {
                    var x = (int) _x;
                    var z = (int) _z;
                    var y = Parent.GetHighestY(x, z);
                    var block = Parent.GetBlockAt(x,y,z);

                    if(block.Type != BlockType.Grass) continue;
                    this.LoopStructures(_x, _z, structs, out _, out var noTreesZone, out _);
                    this.LoopPlateaus(_x, _z, plateaus, out var noTreesPlateau);
                    this.LoopGroundworks(_x, _z, groundworks, out var noTreesGroundwork);
                    
                    if (World.Seed == World.MenuSeed)
                    {
                        //Is menu, force a platform
                        if ((Scenes.MenuBackground.DefaultPosition.Xz - new Vector2(
                                 Parent.OffsetX + x * Chunk.BlockSize,
                                 Parent.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 32 * 32) continue;
                        if ((Scenes.MenuBackground.CreatorPosition.Xz - new Vector2(
                                 Parent.OffsetX + x * Chunk.BlockSize,
                                 Parent.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 32 * 32) continue;
                        if ((Scenes.MenuBackground.CampfirePosition.Xz - new Vector2(
                                 Parent.OffsetX + x * Chunk.BlockSize,
                                 Parent.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 48 * 48) continue;
                    }

                    if(noTreesZone || noTreesPlateau || noTreesGroundwork) continue;

                    var realPosition = new Vector3(Parent.OffsetX + _x * Chunk.BlockSize, y-1, Parent.OffsetZ + _z * Chunk.BlockSize);

                    var region = Cache.GetRegion(realPosition);
                    var noise = (float) World.GetNoise(realPosition.X * 0.005f, realPosition.Z * 0.005f);
                    var placementObject = World.TreeGenerator.CanGenerateTree(realPosition, region);
                    if (!placementObject.Placed) continue;
                    World.TreeGenerator.GenerateTree(placementObject, region, region.Trees.GetDesign( (int) (noise * 10000) ));
                }
            }
        }

        private void LoopPlateaus(int X, int Z, BasePlateau[] RoundedPlateaux, out bool Result)
        {
            Result = false;
            for (var i = 0; i < RoundedPlateaux.Length; i++)
            {
                if (RoundedPlateaux[i].Collides(new Vector2(X * Chunk.BlockSize + OffsetX, Z * Chunk.BlockSize + OffsetZ)))
                {
                    Result = true;
                    return;
                }
            }
        }
        
        private void LoopGroundworks(int X, int Z, IGroundwork[] Groundworks, out bool Result)
        {
            Result = false;
            for (var i = 0; i < Groundworks.Length; i++)
            {
                if (Groundworks[i].Affects(new Vector2(X * Chunk.BlockSize + OffsetX, Z * Chunk.BlockSize + OffsetZ)))
                {
                    Result = true;
                    return;
                }
            }
        }

        private void DoEnvironmentPlacements(Vector3 Position, bool HideEnvironment, Region Biome)
        {
            var designs = Biome.Environment.Designs;
            for (var i = 0; i < designs.Length; i++)
            {
                if(designs[i].CanBeHidden && HideEnvironment) continue;
                if (designs[i].ShouldPlace(Position, this.Parent))
                {
                    var design = designs[i].GetDesign(Position, Parent, Parent.Landscape.RandomGen);
                    World.EnvironmentGenerator.GeneratePlant(Position, Biome, design);
                }
            }
        }

        private void LoopStructures(int X, int Z, CollidableStructure[] Structs,
            out bool NoGrassZone, out bool NoTreesZone, out bool InMerchant)
        {
            NoGrassZone = false;
            NoTreesZone = false;
            InMerchant = false;
            foreach (CollidableStructure structPosition in Structs)
            {
                var possiblePosition = new Vector3(Parent.OffsetX + X * Chunk.BlockSize, 0,
                    Parent.OffsetZ + Z * Chunk.BlockSize);

                if (structPosition.Design is GiantTreeDesign)
                {
                    float radius = structPosition.Mountain.Radius;
                    if ((structPosition.Mountain.Position - possiblePosition.Xz).LengthSquared <
                        radius * .3f * radius * .3f)
                    {
                        NoGrassZone = true;
                    }

                    if (!NoTreesZone && (structPosition.Mountain.Position - possiblePosition.Xz).LengthSquared <
                        radius * 0.5f * radius * 0.5f)
                        NoTreesZone = true;

                }

                if (structPosition.Design is TravellingMerchantDesign)
                {
                    var radius = structPosition.Mountain.Radius;
                    if ((structPosition.Mountain.Position - possiblePosition.Xz).LengthSquared <
                        radius * .5f * radius * .5f)
                        InMerchant = true;
                }
            }
        }
    }
}