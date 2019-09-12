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

        public abstract void BuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float Scale, Vector3 Offset);

        public abstract void BuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset);

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
        
        public static void AddSet(float[][][] Map1, float[] Map2, Func<float, float> Transform)
        {
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var j = 0; j < Map1[i].Length; ++j)
                {
                    for (var k = 0; k < Map1[i][j].Length; ++k)
                    {
                        Map1[i][j][k] += Transform(Map2[(i * Map1.Length * Map1[i].Length) + (j * Map1[i].Length) + k]);
                    }
                }
            }
        }
        
        public static void AddSet(float[][] Map1, float[] Map2, Func<float, float> Transform)
        {
            for (var i = 0; i < Map1.Length; ++i)
            {
                for (var j = 0; j < Map1[i].Length; ++j)
                {
                    Map1[i][j] += Transform(Map2[i * Map1.Length + j]);
                }
            }
        }

        public virtual void Dispose()
        {
            
        }
    }
}
