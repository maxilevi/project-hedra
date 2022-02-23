using System;
using System.Numerics;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.BiomeSystem
{
    public class RegionGeneration : IDisposable
    {
        private readonly BiomeGenerationDesign _design;
        private readonly FastNoiseSIMD _noise;
        private readonly float[][][] _tempDensityMap;
        private readonly float[][] _tempHeightMap;
        private readonly object _tempMapLock;

        public RegionGeneration(int Seed, BiomeGenerationDesign Design)
        {
            _noise = new FastNoiseSIMD(Seed);
            _tempMapLock = new object();
            _tempDensityMap = CreateMap<float>(1, Chunk.Height);
            _tempHeightMap = CreateMap<float>(1);
            _design = Design;
        }

        public bool HasDirt => _design.HasDirt;
        public bool HasRivers => _design.HasRivers;
        public bool HasPaths => _design.HasPaths;

        public void Dispose()
        {
            _noise.Dispose();
        }

        public float GetMaxHeight(float X, float Z)
        {
            //return GetAccurateMaxHeight(X, Z);
            
            lock (_tempMapLock)
            {
                _noise.Seed = World.Seed;
                _tempHeightMap[0][0] = 0;
                _design.BuildHeightMap(_noise, _tempHeightMap, null, 1, Chunk.BlockSize, new Vector2(X, Z));
                return _tempHeightMap[0][0];
            }
        }

        public float RiverAtPoint(float X, float Z)
        {
            lock (_tempMapLock)
            {
                _noise.Seed = World.Seed;
                _tempHeightMap[0][0] = 0;
                _design.BuildRiverMap(_noise, _tempHeightMap, 1, Chunk.BlockSize, new Vector2(X, Z));
                return _tempHeightMap[0][0];
            }
        }

        public float GetAccurateMaxHeight(float X, float Z)
        {
            lock (_tempMapLock)
            {
                _noise.Seed = World.Seed;
                /* Clear buffers */
                _tempHeightMap[0][0] = 0;
                for (var i = 0; i < Chunk.Height; ++i) _tempDensityMap[0][i][0] = 0;

                _design.BuildDensityMap(_noise, _tempDensityMap, null, 1, Chunk.Height, Chunk.BlockSize,
                    Chunk.BlockSize, new Vector3(X, 0, Z));
                _design.BuildHeightMap(_noise, _tempHeightMap, null, 1, Chunk.BlockSize, new Vector2(X, Z));

                var chunkOffset = World.ToChunkSpace(new Vector2(X, Z));
                World.StructureHandler.CheckLandforms(chunkOffset);
                var landformAddon = LandscapeGenerator.HandleLandforms(new Vector2(X, Z), World.WorldBuilding.Landforms);
                
                for (var i = 0; i < Chunk.Height; ++i)
                    _tempDensityMap[0][i][0] =
                        LandscapeGenerator.CalculateDensityForBlock(_tempHeightMap[0][0] + landformAddon, _tempDensityMap[0][i][0], i);
                
                

                for (var i = 0; i < Chunk.Height; ++i)
                    if (_tempDensityMap[0][i][0] < 0)
                    {
                        if (i > 0)
                            return _tempDensityMap[0][i - 1][0] + (i - 1);
                        return _tempDensityMap[0][i][0];
                    }

                return 0;
            }
        }

        public void BuildDensityMap(FastNoiseSIMD Noise, int Width, int Height, float HorizontalScale,
            float VerticalScale, Vector3 Offset, out float[][][] DensityMap, out BlockType[][][] TypeMap)
        {
            DensityMap = CreateMap<float>(Width, Height);
            TypeMap = CreateMap<BlockType>(Width, Height);
            _design.BuildDensityMap(Noise, DensityMap, TypeMap, Width, Height, HorizontalScale, VerticalScale, Offset);
        }

        public void BuildHeightMap(FastNoiseSIMD Noise, int Width, float Scale, Vector2 Offset, out float[][] HeightMap,
            out BlockType[][] TypeMap)
        {
            HeightMap = CreateMap<float>(Width);
            TypeMap = CreateMap<BlockType>(Width);
            _design.BuildHeightMap(Noise, HeightMap, TypeMap, Width, Scale, Offset);
        }

        public void BuildRiverMap(FastNoiseSIMD Noise, int Width, float Scale, Vector2 Offset, out float[][] RiverMap,
            out float[][] RiverBorderMap)
        {
            RiverMap = CreateMap<float>(Width);
            _design.BuildRiverMap(Noise, RiverMap, Width, Scale, Offset);
            RiverBorderMap = CreateMap<float>(Width);
            _design.BuildRiverBorderMap(Noise, RiverBorderMap, Width, Scale, Offset);
        }

        public void BuildPathMap(FastNoiseSIMD Noise, int Width, float Scale, Vector2 Offset, out float[][] PathMap)
        {
            PathMap = CreateMap<float>(Width);
            _design.BuildPathMap(Noise, PathMap, Width, Scale, Offset);
        }

        private T[][][] CreateMap<T>(int Width, int Height)
        {
            var arr = new T[Width][][];
            for (var x = 0; x < Width; ++x)
            {
                arr[x] = new T[Height][];
                for (var y = 0; y < Height; ++y) arr[x][y] = new T[Width];
            }

            return arr;
        }

        private T[][] CreateMap<T>(int Width)
        {
            var arr = new T[Width][];
            for (var x = 0; x < Width; ++x) arr[x] = new T[Width];
            return arr;
        }

        public static RegionGeneration Interpolate(params RegionGeneration[] RegionsGenerations)
        {
            //TODO Implement a good interpolation
            return RegionsGenerations[0];
        }
    }
}