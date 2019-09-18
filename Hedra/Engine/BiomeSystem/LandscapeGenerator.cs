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

        public LandscapeGenerator(Chunk Chunk) : base(Chunk)
        {
        }

        protected override void DefineBlocks(Block[][][] Blocks, RegionCache Cache, int Lod, Func<int, int, bool> Filter)
        {
            var rng = new Random(World.Seed + 1234123);
            
            const int noiseScale = 1;
            var width = (int) (Chunk.Width / Chunk.BlockSize);
            var depth = (int) (Chunk.Width / Chunk.BlockSize);

            var noise3D = FillNoise(width, Chunk.Height);
            var heights = FillHeight(width, out var types);
            var dirtArray = FillDirt(width, Chunk.Biome.Generation);
            var riverMap = FillRiver(width, Chunk.Biome.Generation, out var riverBorderMap);
            var sampledBlocks = CreateSampledBlocks(Blocks);

            var plateaus = World.WorldBuilding.GetPlateausFor(new Vector2(OffsetX, OffsetZ));
            var groundworks = World.WorldBuilding.GetGroundworksFor(new Vector2(OffsetX, OffsetZ)).ToList();
            
            var hasRiver = 0;//biomeGen.HasRivers ? 1f : 0f;
            var hasPath = 0;//biomeGen.HasPaths ? 1f : 0f;

            for (var x = 0; x < width; x+=Lod)
            {

                for (var z = 0; z < depth; z+=Lod)
                {
                    if(!Filter(x,z)) continue;
                    var makeDirt = dirtArray[x][z];
                    var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
                    var height = CalculateHeight(x, z, heights, types, out var type);

                    var densityClampHeight = Chunk.Height - 3f;
                    HandleStructures(position, groundworks, out var blockGroundworks, ref height);
                    HandlePlateaus(x, z, position, plateaus, ref densityClampHeight);

                    /* Water is built from the buttom up so we should not change this */
                    for (var y = 0; y < Chunk.Height; ++y)
                    {
                        var density = CalculateDensity(x,y,z, noise3D);
                        if (y >= densityClampHeight) density = 0;
                        
                        this.GenerateBlock(sampledBlocks, density, type, x, y, z, height, makeDirt, rng);
                                    
                        this.HandleGroundworks(sampledBlocks, x, y, z, blockGroundworks);
                    }
                    GrassBlockPass(sampledBlocks, noise3D, heights, x, z, makeDirt, width, depth);
                }
            }

            CopyBlocks(sampledBlocks, Blocks);
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

        private static void CopyBlocks(SampledBlock[][][] SampledBlocks, Block[][][] Blocks)
        {
            for (var x = 0; x < SampledBlocks.Length; ++x)
            {
                for (var y = 0; y < SampledBlocks[x].Length; ++y)
                {
                    for (var z = 0; z < SampledBlocks[x][y].Length; ++z)
                    {
                        Blocks[x][y][z].Type = SampledBlocks[x][y][z].Type;
                        Blocks[x][y][z].Density = SampledBlocks[x][y][z].Density;
                    }
                }
            }
        }

        private static SampledBlock[][][] CreateSampledBlocks(Block[][][] Blocks)
        {
            var sampled = new SampledBlock[Blocks.Length][][];
            for (var x = 0; x < sampled.Length; ++x)
            {
                sampled[x] = new SampledBlock[Blocks[x].Length][];
                for (var y = 0; y < sampled[x].Length; ++y)
                {
                    sampled[x][y] = new SampledBlock[Blocks[x][y].Length];
                }
            }
            return sampled;
        }

        private float[][] FillRiver(int width, RegionGeneration Biome, out float[][] RiverBorderMap)
        {
            var riverArray = new float[width][];
            var index = 0;
            for (var x = 0; x < riverArray.Length; x++)
            {
                riverArray[x] = new float[width];
            }

            RiverBorderMap = riverArray;
            return riverArray;
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
            Chunk.Biome.Generation.BuildHeightMap(
                noiseValuesWidth,
                Chunk.BlockSize * noise2DScaleWidth,
                new Vector2(Chunk.OffsetX - Chunk.BlockSize * noise2DScaleWidth, Chunk.OffsetZ - Chunk.BlockSize * noise2DScaleWidth),
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
            Chunk.Biome.Generation.BuildDensityMap(
                noiseValuesMapWidth,
                noiseValuesMapHeight,
                Chunk.BlockSize * noise3DScaleWidth,
                Chunk.BlockSize * noise3DScaleHeight,
                new Vector3(Chunk.OffsetX - Chunk.BlockSize * noise3DScaleWidth, 0, Chunk.OffsetZ - Chunk.BlockSize * noise3DScaleWidth),
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
        
        private void GenerateBlock(SampledBlock[][][] Blocks, float density, BlockType Type, int x, int y, int z, float height,
            bool makeDirt, Random rng)
        {
            var currentBlock = Blocks[x][y][z];
            var blockType = BlockType.Air;
            var blockDensity = CalculateDensityForBlock(height, density, y);

            HandleNormalBlocks(Blocks, ref x, ref y, ref z, ref blockDensity, ref blockType, ref makeDirt, ref rng);
            //HandleRiverBlocks(Blocks, ref x, ref y, ref z, ref blockType, ref river, ref height);
            HandleOceanBlocks(Blocks, ref x, ref y, ref z, ref blockType);

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

        private void HandleRiverBlocks(SampledBlock[][][] Blocks, ref int x, ref int y, ref int z, ref BlockType blockType, ref float river, ref float riverBorders, ref float height)
        {
            var riverHeight = height + river + 1.5f;
            if (y < riverHeight)
            {
                if (blockType == BlockType.Air && river > 0)
                {
                    blockType = BlockType.Water;
                    Chunk.AddWaterDensity(new Vector3(x, y, z), (Half) (riverHeight));
                }
                else if (Mathf.Clamp(riverBorders * 100f, 0, RiverDepth) > 2 &&
                         blockType != BlockType.Air)
                {
                    blockType = BlockType.Seafloor;
                    for (var i = 0; i < y; i++) Blocks[x][i][z].Type = BlockType.Seafloor;
                }
            }
        }

        private void HandleOceanBlocks(SampledBlock[][][] Blocks, ref int x, ref int y, ref int z, ref BlockType blockType)
        {
            
            if (y <= BiomePool.SeaLevel && blockType != BlockType.Air && y >= 1)
            {
                var underBlock = Blocks[x][y - 1][z];
                if (underBlock.Type == BlockType.Air || underBlock.Type == BlockType.Water)
                    return;
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
                    Chunk.AddWaterDensity(new Vector3(x, y, z), (Half) BiomePool.SeaLevel);
                }
            }
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

        protected override void PlaceEnvironment(RegionCache Cache, Predicate<PlacementDesign> Filter)
        {
            var structs = World.StructureHandler.StructureItems;
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoPlants).ToArray();
            for (var x = 0; x < this.Chunk.BoundsX; x++)
            {
                for (var z = 0; z < this.Chunk.BoundsZ; z++)
                {
                    this.LoopGroundworks(x, z, groundworks, out var noPlantsGroundwork);
                    if(noPlantsGroundwork) continue;
                    var y = Chunk.GetHighestY(x, z);
                    if(y < BiomePool.SeaLevel - Chunk.BlockSize) continue;
                    var samplingPosition = new Vector3(Chunk.OffsetX + x * Chunk.BlockSize, y-1, Chunk.OffsetZ + z * Chunk.BlockSize);
                    
                    
                    var region = Cache.GetRegion(samplingPosition);
                    this.LoopStructures(x, z, structs, out var noWeedZone, out _, out _);
                    this.DoEnvironmentPlacements(samplingPosition, noWeedZone, region, Filter);
                }
            }
        }

        protected override void DoTreeAndStructurePlacements(Block[][][] Blocks, RegionCache Cache, int Lod)
        {
            var structs = World.StructureHandler.StructureItems;
            var plateaus = World.WorldBuilding.Plateaux.Where(P => P.NoTrees).ToArray();
            var groundworks = World.WorldBuilding.Groundworks.Where(P => P.NoTrees).ToArray();
            for (var _x = 0; _x < this.Chunk.BoundsX; _x++)
            {
                for (var _z = 0; _z < this.Chunk.BoundsZ; _z++)
                {
                    var coordinates = GetNearest(_x, _z, Lod);
                    var x = (int) coordinates.X;
                    var z = (int) coordinates.Y;
                    var y = Chunk.GetHighestY(x, z);
                    var block = Blocks[x][y][z];

                    if(block.Type != BlockType.Grass) continue;
                    this.LoopStructures(_x, _z, structs, out _, out var noTreesZone, out _);
                    this.LoopPlateaus(_x, _z, plateaus, out var noTreesPlateau);
                    this.LoopGroundworks(_x, _z, groundworks, out var noTreesGroundwork);
                    
                    if (World.Seed == World.MenuSeed)
                    {
                        //Is menu, force a platform
                        if ((Scenes.MenuBackground.DefaultPosition.Xz - new Vector2(
                                 Chunk.OffsetX + x * Chunk.BlockSize,
                                 Chunk.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 32 * 32) continue;
                        if ((Scenes.MenuBackground.CreatorPosition.Xz - new Vector2(
                                 Chunk.OffsetX + x * Chunk.BlockSize,
                                 Chunk.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 32 * 32) continue;
                        if ((Scenes.MenuBackground.CampfirePosition.Xz - new Vector2(
                                 Chunk.OffsetX + x * Chunk.BlockSize,
                                 Chunk.OffsetZ + z * Chunk.BlockSize)).LengthSquared < 48 * 48) continue;
                    }

                    if(noTreesZone || noTreesPlateau || noTreesGroundwork) continue;

                    var realPosition = new Vector3(Chunk.OffsetX + _x * Chunk.BlockSize, y-1, Chunk.OffsetZ + _z * Chunk.BlockSize);
                    var samplingPosition = new Vector3(Chunk.OffsetX + x * Chunk.BlockSize, y-1, Chunk.OffsetZ + z * Chunk.BlockSize);
                    
                    var region = Cache.GetRegion(realPosition);
                    var noise = (float) World.GetNoise(realPosition.X * 0.005f, realPosition.Z * 0.005f);
                    var placementObject = World.TreeGenerator.CanGenerateTree(samplingPosition, region, Lod);
                    if (!placementObject.Placed) continue;
                    placementObject.Position += -samplingPosition.Xz.ToVector3() + realPosition.Xz.ToVector3();
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
        
        private Vector2 GetNearest(int X, int Z, int Lod)
        {
            var directionX = X == 0 ? +(X % Lod) : X == Chunk.BoundsX - Lod ? -(X % Lod) : 0; 
            var directionZ = Z == 0 ? +(Z % Lod) : Z == Chunk.BoundsZ - Lod ? -(Z % Lod) : 0;
            return new Vector2(X % Lod == 0 ? X : X + directionX, Z % Lod == 0 ? Z : Z + directionZ);
        }
        

        private void DoEnvironmentPlacements(Vector3 Position, bool HideEnvironment,
            Region Biome, Predicate<PlacementDesign> Filter)
        {
            var designs = Biome.Environment.Designs;
            for (var i = 0; i < designs.Length; i++)
            {
                if(!Filter(designs[i])) continue;
                if(designs[i].CanBeHidden && HideEnvironment) continue;
                if (designs[i].ShouldPlace(Position, this.Chunk))
                {
                    var design = designs[i].GetDesign(Position, Chunk, Chunk.Landscape.RandomGen);
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
                var possiblePosition = new Vector3(Chunk.OffsetX + X * Chunk.BlockSize, 0,
                    Chunk.OffsetZ + Z * Chunk.BlockSize);

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