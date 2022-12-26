/*
 * Author: Zaphyk
 * Date: 17/02/2016
 * Time: 12:00 a.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem.GhostTown;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.BiomeSystem.ShroomDimension;
using Hedra.Engine.BiomeSystem.UndeadBiome;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Numerics;

namespace Hedra.Engine.BiomeSystem
{
    /// <summary>
    ///     Description of BiomePool.
    /// </summary>
    public class BiomePool : IBiomePool
    {
        public const int SeaLevel = 16;
        public const float PathHeight = RiverWaterLevel;
        public const float RiverMaxHeight = RiverWaterLevel - 1;
        public const float RiverMinHeight = SeaLevel - 4;
        public const float RiverMinClipDistance = RiverFloorLevel - 2;
        public const float RiverSeaFloorMax = RiverWaterLevel + 2;
        public const float RiverWaterLevel = 26.5f;
        public const float RiverFloorLevel = RiverWaterLevel - BaseBiomeGenerationDesign.RiverDepth;
        public const int MaxRegionsPerBiome = 8;
        public const float RiverSeaFloorMin = RiverFloorLevel - BaseBiomeGenerationDesign.RiverDepth - 2 - 100;
        private readonly RandomDistribution _biomeDistribution;
        private readonly Dictionary<int, Region> _regionCache;
        private readonly RandomDistribution _regionDistribution;

        private readonly Voronoi _voronoi;
        private BiomeDesign[] _biomeDesigns;
        private Dictionary<WorldType, BiomeDesign[]> _designsMap;
        private Dictionary<WorldType, Type> _generatorMap;

        public BiomePool(WorldType Type)
        {
            this.Type = Type;
            _regionCache = new Dictionary<int, Region>();
            _regionDistribution = new RandomDistribution(true);
            _biomeDistribution = new RandomDistribution(true);
            _voronoi = new Voronoi();
            BuildMappings();
        }

        public BiomeGenerator GetGenerator(Chunk Chunk)
        {
            return (BiomeGenerator)Activator.CreateInstance(_generatorMap[Type], Chunk);
        }

        public Region GetRegion(Vector3 Position)
        {
            lock (_regionCache)
            {
                var voronoiHeight = VoronoiFormula(Position.Xz().ToVector3());

                _regionDistribution.Seed = (int)voronoiHeight;
                var regionIndex = _regionDistribution.Next(0, MaxRegionsPerBiome);

                _biomeDistribution.Seed = (int)voronoiHeight + 421;
                var biomeIndex = _biomeDistribution.Next(0, _biomeDesigns.Length);

                if ((Position - World.SpawnPoint).Xz().LengthFast() < 5000) biomeIndex = 0;

                var index = (regionIndex * 100 / 13 + biomeIndex * 100 / 11) * 100;

                if (_regionCache.ContainsKey(index))
                    return _regionCache[index];

                var regionColors = new RegionColor(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].ColorDesign);
                var regionTrees = new RegionTree(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].TreeDesign);
                var regionStructures = new RegionStructure(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].StructureDesign);
                var regionSky = new RegionSky(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].SkyDesign);
                var regionMob = new RegionMob(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].MobDesign);
                var regionGeneration = new RegionGeneration(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].GenerationDesign);
                var regionEnviroment = new RegionEnviroment(World.Seed + regionIndex + biomeIndex,
                    _biomeDesigns[biomeIndex].EnvironmentDesign);
                var region = new Region
                {
                    Colors = regionColors,
                    Trees = regionTrees,
                    Structures = regionStructures,
                    Sky = regionSky,
                    Mob = regionMob,
                    Generation = regionGeneration,
                    Environment = regionEnviroment
                };

                _regionCache.Add(index, region);

                return _regionCache[index];
            }
        }

        public RegionColor GetAverageRegionColor(Vector3 Offset)
        {
            var regionList = new List<RegionColor>
            {
                GetRegion(Offset).Colors
            };
            for (var x = -1; x < 1 + 1; x++)
            {
                if (x == 0) continue;
                var offset = new Vector2(Offset.X + Chunk.BlockSize * x * 4f, Offset.Z + Chunk.BlockSize * 0 * 4f);
                var region = GetRegion(offset.ToVector3());

                regionList.Add(region.Colors);
            }

            for (var z = -1; z < 1 + 1; z++)
            {
                if (z == 0) continue;
                var offset = new Vector2(Offset.X + Chunk.BlockSize * 0 * 4f, Offset.Z + Chunk.BlockSize * z * 4f);
                var region = GetRegion(offset.ToVector3());

                regionList.Add(region.Colors);
            }

            var firstReg = regionList.FirstOrDefault();
            if (regionList.All(Reg => Reg == firstReg)) return firstReg;
            return RegionColor.Interpolate(regionList.ToArray());
        }

        public Region GetPredominantBiome(Chunk Chunk)
        {
            return GetPredominantBiome(
                new Vector2(Chunk.OffsetX + Chunk.Width * .5f, Chunk.OffsetZ + Chunk.Width * .5f));
        }

        public WorldType Type { get; }

        public BiomeDesign GetBiomeDesign(Vector3 Offset)
        {
            var voronoiHeight = VoronoiFormula(Offset);
            return _biomeDesigns[new Random((int)voronoiHeight).Next(0, _biomeDesigns.Length)];
        }

        public Region GetAverageRegion(Vector3 Offset, float Spacing)
        {
            var regionList = new List<Region>();
            for (var x = -1; x < 1 + 1; x++)
            {
                if (x == 0) continue;
                var offset = new Vector2(Offset.X + Chunk.BlockSize * x * Spacing,
                    Offset.Y + Chunk.BlockSize * 0 * Spacing);
                var region = GetRegion(offset.ToVector3());

                regionList.Add(region);
            }

            for (var z = -1; z < 1 + 1; z++)
            {
                if (z == 0) continue;
                var offset = new Vector2(Offset.X + Chunk.BlockSize * 0 * Spacing,
                    Offset.Y + Chunk.BlockSize * z * Spacing);
                var region = GetRegion(offset.ToVector3());

                regionList.Add(region);
            }

            var firstReg = regionList.FirstOrDefault();
            if (regionList.All(Reg => Reg == firstReg)) return firstReg;

            return Region.Interpolate(regionList.ToArray());
        }

        public float VoronoiFormula(Vector3 Offset)
        {
            var wSeed = World.Seed * 0.0001f;
            return (float)_voronoi.GetValue(Offset.X * .0001f + wSeed, Offset.Z * .0001f + wSeed) * 100f;
        }

        public Region GetPredominantBiome(Vector2 ChunkOffset)
        {
            return GetRegion(new Vector3(ChunkOffset.X, 0, ChunkOffset.Y));
        }

        private void BuildMappings()
        {
            _designsMap = new Dictionary<WorldType, BiomeDesign[]>
            {
                {
                    WorldType.Overworld, new[]
                    {
                        new BiomeDesign
                        {
                            ColorDesign = new NormalBiomeColors(),
                            StructureDesign = new NormalBiomeStructureDesign(),
                            TreeDesign = new NormalBiomeTreeDesign(),
                            SkyDesign = new NormalBiomeSkyDesign(),
                            MobDesign = new NormalBiomeMobDesign(),
                            GenerationDesign = new NormalBiomeGenerationDesign(),
                            EnvironmentDesign = new NormalBiomeEnvironmentDesign()
                        } /*,
                        
                        new BiomeDesign
                        {
                            ColorDesign = new SavannaBiomeColors(),
                            StructureDesign = new SavannaBiomeStructureDesign(),
                            TreeDesign = new SavannaBiomeTreeDesign(),
                            SkyDesign = new SavannaBiomeSkyDesign(),
                            MobDesign = new SavannaBiomeMobDesign(),
                            GenerationDesign = new SavannaBiomeGenerationDesign(),
                            EnvironmentDesign = new SavannaBiomeEnvironmentDesign()
                        }*/
                    }
                },
                {
                    WorldType.GhostTown, new[]
                    {
                        new BiomeDesign
                        {
                            ColorDesign = new GhostTownColorsDesign(),
                            StructureDesign = new GhostTownBiomeStructureDesign(),
                            TreeDesign = new GhostTownTreeDesign(),
                            SkyDesign = new GhostTownSkyDesign(),
                            MobDesign = new GhostTownMobDesign(),
                            GenerationDesign = new GhostTownGenerationDesign(),
                            EnvironmentDesign = new GhostTownEnvironmentDesign()
                        }
                    }
                },
                {
                    WorldType.ShroomDimension, new[]
                    {
                        new BiomeDesign
                        {
                            ColorDesign = new ShroomDimensionColorsDesign(),
                            StructureDesign = new ShroomDimensionBiomeStructureDesign(),
                            TreeDesign = new ShroomDimensionTreeDesign(),
                            SkyDesign = new ShroomDimensionSkyDesign(),
                            MobDesign = new ShroomDimensionMobDesign(),
                            GenerationDesign = new ShroomDimensionGenerationDesign(),
                            EnvironmentDesign = new ShroomDimensionEnvironmentDesign()
                        }
                    }
                }
            };
            _generatorMap = new Dictionary<WorldType, Type>
            {
                { WorldType.Overworld, typeof(LandscapeGenerator) },
                { WorldType.GhostTown, typeof(LandscapeGenerator) },
                { WorldType.ShroomDimension, typeof(LandscapeGenerator) }
            };
            _biomeDesigns = _designsMap[Type];
        }
    }
}