using System;
using System.Numerics;
using Hedra.Engine.Generation;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeGenerationDesign
    {
        public abstract bool HasRivers { get; }
        public abstract bool HasPaths { get; }
        public abstract bool HasDirt { get; }
        
        public virtual float DirtFrequency => 0.005f;
        public virtual float DirtThreshold => 0.3f;

        public virtual void BuildDensityMap(FastNoiseSIMD Noise, float[][][] DensityMap, BlockType[][][] TypeMap,
            int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            DoBuildDensityMap(Noise, DensityMap, TypeMap, Width, Height, HorizontalScale, VerticalScale, Offset);
        }

        public virtual void BuildHeightMap(FastNoiseSIMD Noise, float[][] HeightMap, BlockType[][] TypeMap, int Width,
            float Scale, Vector2 Offset)
        {
            DoBuildHeightMap(Noise, HeightMap, TypeMap, Width, Scale, Offset);
        }

        protected abstract void DoBuildDensityMap(FastNoiseSIMD Noise, float[][][] DensityMap, BlockType[][][] TypeMap,
            int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset);

        protected abstract void DoBuildHeightMap(FastNoiseSIMD Noise, float[][] HeightMap, BlockType[][] TypeMap,
            int Width, float Scale, Vector2 Offset);

        protected abstract void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale,
            Vector2 Offset);

        protected abstract void DoBuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale,
            Vector2 Offset);

        protected abstract void DoBuildPathMap(FastNoiseSIMD Noise, float[][] PathMap, int Width, float Scale,
            Vector2 Offset);

        public void BuildPathMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            DoBuildPathMap(Noise, RiverMap, Width, Scale, Offset);
        }

        public void BuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            DoBuildRiverMap(Noise, RiverMap, Width, Scale, Offset);
        }

        public void BuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            DoBuildRiverBorderMap(Noise, RiverMap, Width, Scale, Offset);
        }

        public void AddFunction(float[][][] Map1, Func<int, int, int, float> Function)
        {
            for (var i = 0; i < Map1.Length; ++i)
            for (var j = 0; j < Map1[i].Length; ++j)
            for (var k = 0; k < Map1[i][j].Length; ++k)
                Map1[i][j][k] += Function(i, j, k);
        }

        public void AddFunction(float[][] Map1, Func<int, int, float> Function)
        {
            for (var i = 0; i < Map1.Length; ++i)
            for (var k = 0; k < Map1[i].Length; ++k)
                Map1[i][k] += Function(i, k);
        }

        public void AddConstant(float[][] Map1, float Constant)
        {
            AddFunction(Map1, (X, Z) => Constant);
        }

        public static void AddSet(float[][][] Map1, float[] Map2, Func<Vector<float>, Vector<float>> Transform)
        {
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var j = 0; j < Map1[i].Length; ++j)
                {
                    var length = Map1[i][j].Length;
                    var remaining = length % Vector<float>.Count;
                    for (var k = 0; k < length - remaining; k += Vector<float>.Count)
                    {
                        var baseIdx = i * Map1[i].Length * Map1[i][j].Length + j * Map1[i][j].Length + k;
                        (Transform(new Vector<float>(Map2, baseIdx)) + new Vector<float>(Map1[i][j], k)).CopyTo(Map1[i][j], k);
                    }

                    for (var k = length - remaining; k < length; ++k)
                    {
                        var baseIdx = i * Map1[i].Length * Map1[i][j].Length + j * Map1[i][j].Length + k;
                        Map1[i][j][k] += Transform(new Vector<float>(Map2[baseIdx]))[0];
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

        public static float[] OperateSetsDimensional(float[] Set3D, float[] Set2D, Vector3 Size,
            Func<Vector<float>, Vector<float>, Vector<float>> Operation)
        {
            var length = (int) Size.Z;
            var remaining = length % Vector<float>.Count;
            
            for (var x = 0; x < Size.X; ++x)
            {
                for (var y = 0; y < Size.Y; ++y)
                {
                    for (var z = 0; z < length - remaining; z += Vector<float>.Count)
                    {
                        var idx3D = x * (int)Size.Y * (int)Size.Z + y * (int)Size.Z + z;
                        var idx2D = x * (int)Size.Z + z;
                        Operation(new Vector<float>(Set3D, idx3D), new Vector<float>(Set2D, idx2D)).CopyTo(Set3D, idx3D);
                    }
                    
                    for (var z = length - remaining; z < length; ++z)
                    {
                        var idx3D = x * (int)Size.Y * (int)Size.Z + y * (int)Size.Z + z;
                        var idx2D = x * (int)Size.Z + z;
                        Set3D[idx3D] = Operation(new Vector<float>(Set3D[idx3D]), new Vector<float>(Set2D[idx2D]))[0];
                    }
                }
            }

            return Set3D;
        }

        public static float[] OperateSets(float[] Set1, float[] Set2,
            Func<Vector<float>, Vector<float>, Vector<float>> Operation)
        {
            var length = Set1.Length;
            var remaining = length % Vector<float>.Count;
            for (var i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                var v1 = new Vector<float>(Set1, i);
                var v2 = new Vector<float>(Set2, i);
                Operation(v1, v2).CopyTo(Set1, i);
            }

            for (var i = length - remaining; i < length; ++i)
            {
                Set1[i] = Operation(new Vector<float>(Set1[i]), new Vector<float>(Set2[i]))[0];
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

        public static float[] TransformSet(float[] Set, Func<Vector<float>, Vector<float>> Transform)
        {
            var length = Set.Length;
            var remaining = length % Vector<float>.Count;
            for (var i = 0; i < length - remaining; i += Vector<float>.Count)
            {
                Transform(new Vector<float>(Set, i)).CopyTo(Set, i);
            }
            
            for (var i = length - remaining; i < length; ++i)
            {
                Set[i] = Transform(new Vector<float>(Set[i]))[0];
            }

            return Set;
        }

        public static void AddSet(float[][] Map1, float[] Map2, Func<float, float> Transform)
        {
            var index = 0;
            for (var i = 0; i < Map1.Length; ++i)
            for (var j = 0; j < Map1[i].Length; ++j)
                Map1[i][j] += Transform(Map2[index++]);
        }
    }
}