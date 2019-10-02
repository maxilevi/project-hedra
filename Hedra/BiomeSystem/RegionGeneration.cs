using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.BiomeSystem
{
    public class RegionGeneration : IDisposable
    {
        private readonly BiomeGenerationDesign _design;
        private readonly object _tempMapLock;
        private readonly float[][] _tempDensityMap;
        private readonly BlockType[][] _tempTypeMap;
        private readonly FastNoiseSIMD _noise;

        public RegionGeneration(int Seed, BiomeGenerationDesign Design)
        {
            _noise = new FastNoiseSIMD(Seed);
            _tempMapLock = new object();
            _tempDensityMap = CreateMap<float>(1);
            _tempTypeMap = CreateMap<BlockType>(1);
            _design = Design;
        }

        public bool HasDirt => _design.HasDirt;
        public bool HasRivers => _design.HasRivers;
        public bool HasPaths => _design.HasPaths;

        public float GetHeight(float X, float Y, out BlockType Type)
        {
            lock (_tempMapLock)
            {
                _tempDensityMap[0][0] = 0;
                _design.BuildHeightMap(_noise, _tempDensityMap, _tempTypeMap, 1, 1, new Vector2(X, Y));
                Type = _tempTypeMap[0][0];
                return _tempDensityMap[0][0];
            }
        }
        
        public void BuildDensityMap(FastNoiseSIMD Noise, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset, out float[][][] DensityMap, out BlockType[][][] TypeMap)
        {
            DensityMap = CreateMap<float>(Width, Height);
            TypeMap = CreateMap<BlockType>(Width, Height);
            _design.BuildDensityMap(Noise, DensityMap, TypeMap, Width, Height, HorizontalScale, VerticalScale, Offset);
        }

        public void BuildHeightMap(FastNoiseSIMD Noise, int Width, float Scale, Vector2 Offset, out float[][] HeightMap, out BlockType[][] TypeMap)
        {
            HeightMap = CreateMap<float>(Width);
            TypeMap = CreateMap<BlockType>(Width);
            _design.BuildHeightMap(Noise, HeightMap, TypeMap, Width, Scale, Offset);
        }

        public void BuildRiverMap(FastNoiseSIMD Noise, int Width, float Scale, Vector2 Offset, out float[][] RiverMap, out float[][] RiverBorderMap)
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
                for (var y = 0; y < Height; ++y)
                {
                    arr[x][y] = new T[Width];
                }
            }

            return arr;
        }

        private T[][] CreateMap<T>(int Width)
        {
            var arr = new T[Width][];
            for (var x = 0; x < Width; ++x)
            {
                arr[x] = new T[Width];
            }
            return arr;
        }
        
        public static RegionGeneration Interpolate(params RegionGeneration[] RegionsGenerations)
        {
            //TODO Implement a good interpolation
            return RegionsGenerations[0];
        }

        public void Dispose()
        {
            _noise.Dispose();
        }
    }
}
