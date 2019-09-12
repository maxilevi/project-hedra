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
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using BulletSharp;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.Generation;
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

            var plateaus = World.WorldBuilding.GetPlateausFor(new Vector2(OffsetX, OffsetZ));
            var groundworks = World.WorldBuilding.GetGroundworksFor(new Vector2(OffsetX, OffsetZ)).ToList();

            var structs = World.StructureHandler.StructureItems;
            var heightCache = new Dictionary<Vector2, float[]>();

            var biomeGen = Chunk.Biome.Generation;
            var hasRiver = 0;//biomeGen.HasRivers ? 1f : 0f;
            var hasPath = 0;//biomeGen.HasPaths ? 1f : 0f;
            
            for (var x = 0; x < width; x+=Lod)
            {

                for (var z = 0; z < depth; z+=Lod)
                {
                    if(!Filter(x,z)) continue;
                    var position = new Vector2(x * Chunk.BlockSize + OffsetX, z * Chunk.BlockSize + OffsetZ);
                    var height = CalculateHeight(x, z, heights, types, out var type);

                    this.HandleStructures(x, z, position, groundworks, plateaus, structs, heightCache, biomeGen,
                        hasPath, hasRiver, noiseScale, ref height, out var town, out var makeDirt,
                        out var pathClamped, out var river, out var path, out var riverBorders,
                        out var blockGroundworks, out var isMount, out var mountHeight);

                    for (var y = Chunk.Height-1 -1; y > -1; --y)
                    {
                        var density = CalculateDensity(x,y,z, noise3D);
                        
                        this.GenerateBlock(Blocks, density, type, x, y, z, height, river, makeDirt, riverBorders,
                            rng, isMount, mountHeight);
                                    
                        this.HandleGroundworks(Blocks, x, y, z, path, PathDepth, pathClamped, town,
                            blockGroundworks, ref height);
                    }
                    //GrassBlockPass(Blocks, x, z, makeDirt);
                }
            }
        }

        private static void GrassBlockPass(Block[][][] Blocks, int x, int z, bool makeDirt)
        {
            var foundBlock = false;
            var counter = 0;
            for (var y = Chunk.Height - 2; y > 0; --y)
            {
                var type = Blocks[x][y][z].Type;
                if (type != BlockType.Air && Blocks[x][y+1][z].Type == BlockType.Air && !foundBlock)
                {
                    foundBlock = true;
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
                if (makeDirt && Blocks[x][y][z].Type == BlockType.Grass)
                {
                    Blocks[x][y][z].Type = BlockType.Dirt;
                }
            }
        }


        private float[][] FillHeight(int width, out BlockType[][] types)
        {
            var noiseValuesWidth = width / noise2DScaleWidth + 1;
            Chunk.Biome.Generation.BuildHeightMap(
                noiseValuesWidth,
                Chunk.BlockSize * noise2DScaleWidth,
                new Vector2(Chunk.OffsetX, Chunk.OffsetZ),
                out var heights,
                out types
            );
            return heights;
        }
        
        private float[][][] FillNoise(int width, int height)
        {
            /* We need to use the same scale for the noises because of FastNoiseSIMD*/
            var noiseValuesMapWidth = (width / noise3DScaleWidth) + 1;
            var noiseValuesMapHeight = (height / noise3DScaleWidth) + 1;
            Chunk.Biome.Generation.BuildDensityMap(
                noiseValuesMapWidth,
                noiseValuesMapHeight,
                Chunk.BlockSize * noise3DScaleWidth,
                new Vector3(Chunk.OffsetX, 0, Chunk.OffsetZ),
                out var densityMap,
                out var typeMap
            );
            return densityMap;
        }

        private static float CalculateDensity(int x, int y, int z, float[][][] noise3D)
        {
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
            int x2 = (x / noise2DScaleWidth);
            int z2 = (z / noise2DScaleWidth);
            Type = types[x2][z2];
            return Mathf.LinearInterpolate2D(
                heights[x2][z2], heights[x2 + 1][z2], heights[x2][z2 + 1], heights[x2 + 1][z2 + 1],
                (x % noise2DScaleWidth) / (float) noise2DScaleWidth,
                (z % noise2DScaleWidth) / (float) noise2DScaleWidth
            );
        }
        
        private void GenerateBlock(Block[][][] Blocks, float density, BlockType Type, int x, int y, int z, float height, float river,
            bool makeDirt, float riverBorders, Random rng, bool isMount, float mountHeight)
        {
            var currentBlock = Blocks[x][y][z];
            var blockType = BlockType.Air;
            var blockDensity = 1 - (y - height) + density;

            if (y < 2)
                blockDensity = new Half(0.95f + rng.NextFloat() * 0.75f);
    
            if (blockDensity > 0)
            {
                blockType = BlockType.Stone;
/*
                if (height - y < 18f)
                {
                    blockType = BlockType.Grass;
                }*/
                
                if (y < 2)
                    blockType = BlockType.Seafloor;

                if (isMount && y >= mountHeight)
                {
                    blockType = BlockType.Grass;
                }
            }
            
            if (blockType == BlockType.Grass)
            {
                if (makeDirt) blockType = BlockType.Dirt;
            }

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
    
            if (y <= BiomePool.SeaLevel && blockType == BlockType.Air && y >= 1 &&
                Blocks[x][y - 1][z].Type != BlockType.Seafloor && Blocks[x][y - 1][z].Type != BlockType.Air &&
                Blocks[x][y - 1][z].Type != BlockType.Water)
            {
                if (y < BiomePool.SeaLevel)
                {
                    blockType = BlockType.Seafloor;
                    for (var i = 0; i < y; i++)
                    {
                        Blocks[x][i][z].Type = BlockType.Seafloor;
                    }
                }
            }
    
            var isOcean = y > 0 && y <= BiomePool.SeaLevel &&
                          (Blocks[x][y - 1][z].Type == BlockType.Seafloor ||
                           Blocks[x][y - 1][z].Type == BlockType.Water) &&
                          blockType == BlockType.Air &&
                          Blocks[x][y + 1][z].Type == BlockType.Air;
            if (isOcean)
            {
                blockType = BlockType.Water;
                Chunk.AddWaterDensity(new Vector3(x, y, z), (Half) BiomePool.SeaLevel);
            }

            currentBlock.Type = blockType;
            currentBlock.Density = new Half(blockDensity);
            Blocks[x][y][z] = currentBlock;
        }

        private void HandleStructures(int x, int z, Vector2 position, List<IGroundwork> groundworks, BasePlateau[] RoundedPlateaux,
            CollidableStructure[] structs, Dictionary<Vector2, float[]> heightCache, RegionGeneration biomeGen,
            float hasPath, float hasRiver, float noiseScale, ref float height, out bool town, out bool makeDirt, out bool pathClamped, out float river,
            out float path, out float riverBorders, out IGroundwork[] blockGroundworks, out bool isMount, out float mountHeight)
        {
            town = false;
            CollidableStructure nearCollidableStructure = null;
            float townHeight = 0;

            for(var i = 0; i < structs.Length; i++)
            {
                var item = structs[i];
                if (!(item.Design is VillageDesign)) continue;

                float villageRadius = item.Mountain.Radius;
                if (!((position - item.Position.Xz).LengthSquared < villageRadius * villageRadius)) continue;

                town = true;
                nearCollidableStructure = item;
                break;
            }
            var smallFrequency = SmallFrequency(position.X, position.Y);
            var nearGiantTree = FindNearestGiantTree(position, structs);
            
            blockGroundworks = groundworks.Where(G => G.Affects(position)).ToArray();

            pathClamped = false;
            path = hasPath * PathFormula(Chunk.BlockSize * x + Chunk.OffsetX, Chunk.BlockSize * z + Chunk.OffsetZ);
            path = Mathf.Clamp(path * 100f, 0, PathDepth);

            river = hasRiver * River(position);
            riverBorders = hasRiver * River(position, Border);
            var amplifiedRiverBorders = Mathf.Clamp(riverBorders * RiverMult, 0, RiverDepth);

            river = Mathf.Clamp(river * RiverMult, 0, RiverDepth);

            float riverLerp = amplifiedRiverBorders / RiverDepth;
            if (riverLerp > 0 || path > 0)
            {
                if (heightCache.ContainsKey(position))
                {
                    var cache = heightCache[position][0];
                    height -= Mathf.Lerp(0, cache, Math.Min(1, Math.Min(path * 0.5f, 1) + riverLerp));
                }
            }

            isMount = false;
            mountHeight = 0;
            if (heightCache.ContainsKey(position) && heightCache[position].Length == 2)
            {
                isMount = true;
                mountHeight = heightCache[position][1];
            }
            
            HandlePlateaus(x, z, smallFrequency, ref height, RoundedPlateaux, nearGiantTree, nearCollidableStructure,
                ref river, ref riverBorders, ref path, ref pathClamped);

            height = Math.Max(0, height);
            /*
            for (var i = 0; i < noise3D.Length; i++)
            {

                /*
                if (inPlateau)
                {
                    foreach (Plateau plateau in plateauPositions)
                    {
                        float plateauDist =
                        (plateau.Position.Xz -
                         new Vector2(OffsetX + x * Chunk.BlockSize, OffsetZ + z * Chunk.BlockSize)).LengthFast;
                        float plateauFinal = Math.Max(1 - plateauDist / plateau.Radius, 0);
                        noise3D[i] = Mathf.Lerp(0, noise3D[i], 1 - Math.Min(1, plateauFinal * 3f));
                    }
                }

                if (nearGiantTree == null)
                {
                    const int lerpFactor = 80;
                    noise3D[i] = Mathf.Lerp(noise3D[i], 0f, (float)Mathf.Clamp(riverBorders * lerpFactor, 0, 1));

                    /*noise3D[i] = Mathf.Clamp(
                        Mathf.Lerp(noise3D[i], 0f, path),
                        0, noise3D[i]);
                }
            }*/

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
                height += bonusHeight;// * blockGroundworksModifier;
            }
            if (pathGroundwork != null)
            {
                river = Mathf.Lerp(river, 0, groundworkDensity);
            }
            height -= river;
            height -= Mathf.Lerp(path, 0, riverLerp);
            var dirtNoise = World.GetNoise((x * Chunk.BlockSize + OffsetX) * 0.0175f,
                                (z * Chunk.BlockSize + OffsetZ) * 0.0175f) > .35f;
            makeDirt = biomeGen.HasDirt && dirtNoise;                    
        }

        private void HandlePlateaus(int X, int Z, float smallFrequency, ref float height, BasePlateau[] Plateaux, CollidableStructure nearGiantTree,
            CollidableStructure nearCollidableStructure, ref float river, ref float riverBorders, ref float path, ref bool pathClamped)
        {
            for (var i = 0; i < Plateaux.Length; i++)
            {
                var point = new Vector2(OffsetX + X * Chunk.BlockSize, OffsetZ + Z * Chunk.BlockSize);
                height = Plateaux[i].Apply(point, height, out var final, smallFrequency);
                
                if (nearGiantTree == null)
                {
                    river = Mathf.Clamp(Mathf.Lerp(0, river, 1 - Math.Min(1, final * 3f)), 0, river);
                    riverBorders = Mathf.Clamp(Mathf.Lerp(0, riverBorders, 1 - Math.Min(1, final * 3f)), 0,
                        riverBorders);
                }

                if (path > 0 && nearCollidableStructure != null && Plateaux[i] == nearCollidableStructure.Mountain)
                    pathClamped = true;
                
                path = Mathf.Clamp(Mathf.Lerp(0, path, 1 - Math.Min(1, final * 3f)), 0, path);
            }
        }

        private static CollidableStructure FindNearestGiantTree(Vector2 position, CollidableStructure[] structs)
        {
            for(var i = 0; i < structs.Length; i++)
            {
                float radius = structs[i].Mountain?.Radius ?? float.MaxValue;
                if ((position - structs[i].Position.Xz).LengthSquared < radius * radius && structs[i].Design is GiantTreeDesign) return structs[i];
            }
            return null;
        }
        
        private void HandleGroundworks(Block[][][] Blocks, int X, int Y, int Z, float path, float pathDepth,
            bool pathClamped, bool town, IGroundwork[] BlockGroundworks, ref float Height)
        {
            if (BlockGroundworks.Length > 0 || path == pathDepth || town)
            {
                if (this.IsBlockChangeable(Blocks[X][Y][Z].Type))
                {
                    if (path > 0 && !town || pathClamped && !town)
                    {
                        Blocks[X][Y][Z].Type = BlockType.Path;
                    }

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
                    if (town)
                    {
                        if (Blocks[X][Y][Z].Type == BlockType.Stone)
                        {
                            Blocks[X][Y][Z].Type = BlockType.Grass;
                        }
                    }
                }
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
            return;
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
            return;
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