/*
 * Author: Zaphyk
 * Date: 17/02/2016
 * Time: 12:00 a.m.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.BiomeSystem.SnowBiome;
using Hedra.Engine.BiomeSystem.UndeadBiome;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
	/// <summary>
	/// Description of BiomePool.
	/// </summary>
	public class BiomePool
	{
		public const int WorldWidth = 4096;
		public const int WorldHeight = 4096;
		public const int SeaLevel = 16;
	    public const int MaxRegionsPerBiome = 8;

        public BiomeDesign[] BiomeDesigns;
	    private readonly Voronoi _voronoi;
        private readonly Dictionary<int, Region> _regionCache;

        public BiomePool()
        {
            _regionCache = new Dictionary<int, Region>();

            BiomeDesigns = new BiomeDesign[1];
            /*
             BiomeDesigns[1] = new BiomeDesign
            {
                ColorDesign = new UndeadBiomeColorsDesign(),
                StructureDesign = new UndeadBiomeStructureDesign(),
                TreeDesign = new UndeadBiomeTreeDesign(),
                SkyDesign = new UndeadBiomeSkyDesign(),
                MobDesign = new UndeadBiomeMobDesign(),
                GenerationDesign = new UndeadBiomeGenerationDesign(),
                EnviromentDesign = new UndeadBiomeEnviromentDesign()
            };*/

            BiomeDesigns[0] = new BiomeDesign
	        {
	            ColorDesign = new NormalBiomeColors(),
	            StructureDesign = new NormalBiomeStructureDesign(),
                TreeDesign = new NormalBiomeTreeDesign(),
	            SkyDesign = new NormalBiomeSkyDesign(),
	            MobDesign = new NormalBiomeMobDesign(),
                GenerationDesign = new NormalBiomeGenerationDesign(),
                EnviromentDesign = new NormalBiomeEnviromentDesign()
            };/*
            BiomeDesigns[1] = new BiomeDesign
            {
                ColorDesign = new SnowBiomeColorsDesign(),
                StructureDesign = new SnowBiomeStructureDesign(),
                TreeDesign = new SnowBiomeTreeDesign(),
                SkyDesign = new SnowBiomeSkyDesign(),
                MobDesign = new SnowBiomeMobDesign(),
                GenerationDesign = new SnowBiomeGenerationDesign(),
                EnviromentDesign = new SnowBiomeEnviromentDesign()
            };*/
            _voronoi = new Voronoi
	        {
	            //Seed = World.Seed
	        };
	    }

	    public BiomeDesign GetBiomeDesign(Vector3 Offset)
	    {
	        var voronoiHeight = this.VoronoiFormula(Offset);
	        return BiomeDesigns[new Random((int) voronoiHeight).Next(0, BiomeDesigns.Length)];
	    }

	    public Region GetRegion(Vector3 Position)
	    {
            lock (_regionCache) {
	            var voronoiHeight = this.VoronoiFormula(Position.Xz.ToVector3());
	            int regionIndex = new Random((int) voronoiHeight).Next(0, MaxRegionsPerBiome);
                int biomeIndex = new Random((int) voronoiHeight + 421).Next(0, BiomeDesigns.Length);
                if ((Position.Xz - GameSettings.SpawnPoint).LengthFast < 5000) biomeIndex = 0;

                int index = (regionIndex * 100 / 13 + biomeIndex * 100 / 11) * 100;

                if (_regionCache.ContainsKey(index))
	                return _regionCache[index];

	            var regionColors = new RegionColor(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].ColorDesign);
	            var regionTrees = new RegionTree(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].TreeDesign);
	            var regionStructures = new RegionStructure(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].StructureDesign);
	            var regionSky = new RegionSky(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].SkyDesign);
	            var regionMob = new RegionMob(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].MobDesign);
                var regionGeneration = new RegionGeneration(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].GenerationDesign);
                var regionEnviroment = new RegionEnviroment(World.Seed + regionIndex + biomeIndex, BiomeDesigns[biomeIndex].EnviromentDesign);
                var region = new Region
	            {
	                Colors = regionColors,
	                Trees = regionTrees,
	                Structures = regionStructures,
	                Sky = regionSky,
	                Mob = regionMob,
                    Generation = regionGeneration,
                    Enviroment = regionEnviroment,
                };

	            _regionCache.Add(index, region);

	            return _regionCache[index];
	        }
	    }

	    public Region GetAverageRegion(Vector3 Offset, float Spacing)
	    {
	        var regionList = new List<Region>();
	        for (int x = -1; x < 1 + 1; x++)
	        {
	            if (x == 0) continue;
	            var offset = new Vector2(Offset.X + Chunk.BlockSize * x * Spacing, Offset.Y + Chunk.BlockSize * 0 * Spacing);
	            var region = this.GetRegion(offset.ToVector3());

	            regionList.Add(region);
	        }

	        for (int z = -1; z < 1 + 1; z++)
	        {
	            if (z == 0) continue;
	            var offset = new Vector2(Offset.X + Chunk.BlockSize * 0 * Spacing, Offset.Y + Chunk.BlockSize * z * Spacing);
	            var region = this.GetRegion(offset.ToVector3());

	            regionList.Add(region);
	        }

	        var firstReg = regionList.FirstOrDefault();
	        if (regionList.All(Reg => Reg == firstReg)) return firstReg;

	        return Region.Interpolate(regionList.ToArray());
	    }

        public RegionColor GetAverageRegionColor(Vector3 Offset)
	    {
	        var regionList = new List<RegionColor>
	        {
	            this.GetRegion(Offset).Colors
	        };
	        for (int x = -1; x < 1 + 1; x++)
	        {
	            if (x == 0) continue;
                var offset = new Vector2(Offset.X + Chunk.BlockSize * x * 4f, Offset.Z + Chunk.BlockSize * 0 * 4f);
	            var region = this.GetRegion(offset.ToVector3());

	            regionList.Add(region.Colors);
            }

	        for (int z = -1; z < 1 + 1; z++)
	        {
                if(z == 0) continue;
	            var offset = new Vector2(Offset.X + Chunk.BlockSize * 0 * 4f, Offset.Z + Chunk.BlockSize * z * 4f);
	            var region = this.GetRegion(offset.ToVector3());

	            regionList.Add(region.Colors);
	        }

            var firstReg = regionList.FirstOrDefault();
	        if (regionList.All(Reg => Reg == firstReg)) return firstReg;
            return RegionColor.Interpolate(regionList.ToArray());
        }

	    public float VoronoiFormula(Vector3 Offset)
	    {
	        float wSeed = World.Seed * 0.0001f;
	        return (float) _voronoi.GetValue(Offset.X * .0001f + wSeed, Offset.Z * .0001f + wSeed) * 100f;

	    }

	    public Region GetPredominantBiome(Chunk Chunk)
	    {
	        return this.GetPredominantBiome(new Vector2(Chunk.OffsetX + Chunk.Width * .5f, Chunk.OffsetZ + Chunk.Width * .5f));
	    }

        public Region GetPredominantBiome(Vector2 ChunkOffset)
        {
            return this.GetRegion(new Vector3(ChunkOffset.X, 0, ChunkOffset.Y));
		}

        public static float EncodeWater(float Height, float Density)
        {
            float riverPack = (float)Math.Round(Height, 3) * 0.1f;
            float densityPack = (float)Math.Round(Density, 3) * 0.1f;

            var scaleFactor = 65530.0;
            var cp = 256.0 * 256.0;

            var x1 = (int)(densityPack * scaleFactor);
            var y1 = (int)(riverPack * scaleFactor);
            return (float)(y1 * cp) + x1;
        }

	    public static float DecodeWater(double Den)
	    {
	        var scaleFactor = 65530.0;
	        double cp = 256.0 * 256.0;

	        double dy = Math.Floor(Den / cp);
	        double dx = Den - dy * cp;
	        float xk = -1.0f + (float)(dx / scaleFactor);
	        //We uncompress the floats
	        if (dx == 0)
	            return (float)dx;
	        return xk * 10.0f;
	    }
    }
}
