using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeGenerationDesign : IDisposable
    {
        public abstract bool HasRivers { get;}
        public abstract bool HasPaths { get; }
        public abstract bool HasDirt { get; }

        protected readonly FastNoiseSIMD Noise;
        private readonly object noiseLock = new object();

        protected BiomeGenerationDesign()
        {
            Noise = new FastNoiseSIMD(1);
        }

        public void BuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            lock(noiseLock)
                DoBuildDensityMap(DensityMap, TypeMap, Width, Height, HorizontalScale, VerticalScale, Offset);
        }

        public void BuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            lock(noiseLock)
                DoBuildHeightMap(HeightMap, TypeMap, Width, Scale, Offset);
        }
        
        protected abstract void DoBuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset);

        protected abstract void DoBuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset);

        protected abstract void DoBuildRiverMap(float[][] RiverMap, int Width, float Scale, Vector2 Offset);
        
        public void BuildRiverMap(float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            lock (noiseLock)
                DoBuildRiverMap(RiverMap, Width, Scale, Offset);
        }
        
        public void AddFunction(float[][][] Map1, Func<int, int, int, float> Function)
        {
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var j = 0; j < Map1[i].Length; ++j)
                {
                    for (var k = 0; k < Map1[i][j].Length; ++k)
                    {
                        Map1[i][j][k] += Function(i, j, k);
                    }
                }
            }
        }

        public void AddFunction(float[][] Map1, Func<int, int, float> Function)
        {
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var k = 0; k < Map1[i].Length; ++k)
                {
                    Map1[i][k] += Function(i, k);

                }
            }
        }

        public void AddConstant(float[][] Map1, float Constant)
        {
            AddFunction(Map1, (X,Z) => Constant);
        }

        public static void AddSet(float[][][] Map1, float[] Map2, Func<float, float> Transform)
        {
            var index = 0;
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var j = 0; j < Map1[i].Length; ++j)
                {
                    for (var k = 0; k < Map1[i][j].Length; ++k)
                    {
                        Map1[i][j][k] += Transform(Map2[i * Map1[i].Length * Map1[i][j].Length + j * Map1[i][j].Length + k]);
                    }
                }
            }
        }
        
        public static float[] MultiplySetsDimensional(float[] Set3D, float[] Set2D, Vector3 Size)
        {
            return OperateSetsDimensional(Set3D, Set2D, Size, (F1, F2) => F1 * F2);
        }
        
        public static float[] MultiplySets(float[] Set1, float[] Set2)
        {
            return OperateSets(Set1, Set2, (F1, F2) => F1 * F2);
        }
        
        public static float[] OperateSetsDimensional(float[] Set3D, float[] Set2D, Vector3 Size, Func<float, float, float> Operation)
        {
            var index = 0;
            for (var x = 0; x < Size.X; ++x)
            {
                for (var y = 0; y < Size.Y; ++y)
                {
                    for (var z = 0; z < Size.Z; ++z)
                    {
                        Set3D[x * (int)Size.Y * (int)Size.Z + y * (int)Size.Z + z] = Operation(
                            Set3D[x * (int)Size.Y * (int)Size.Z + y * (int)Size.Z + z],
                            Set2D[x * (int)Size.Z + z]
                        );
                    }
                }   
            }
            return Set3D;
        }
        
        public static float[] OperateSets(float[] Set1, float[] Set2, Func<float, float, float> Operation)
        {
            var index = 0;
            for (var i = 0; i < Set1.Length; ++i)
            {
                Set1[i] = Operation(Set1[i], Set2[i]);
            }
            return Set1;
        }
        
        public static float[] SubtractSets(float[] Set1, float[] Set2)
        {
            return OperateSets(Set1, Set2, (F1, F2) => F1 - F2);
        }
        
        public static float[] AddSets(float[] Set1, float[] Set2)
        {
            return OperateSets(Set1, Set2, (F1, F2) => F1 + F2);
        }

        public static float[] TransformSet(float[] Set, Func<float, float> Transform)
        {
            var index = 0;
            for (var i = 0; i < Set.Length; ++i)
            {
                Set[i] = Transform(Set[i]);
            }

            return Set;
        }
        
        public static void AddSet(float[][] Map1, float[] Map2, Func<float, float> Transform)
        {
            var index = 0;
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var j = 0; j < Map1[i].Length; ++j)
                {
                    Map1[i][j] += Transform(Map2[index++]);
                }
            }
        }

        public int Seed
        {
            set
            {
                lock(noiseLock)
                    Noise.Seed = value;
            }
        }

        public virtual void Dispose()
        {
            lock(noiseLock)
                Noise.Dispose();
        }
    }
}
