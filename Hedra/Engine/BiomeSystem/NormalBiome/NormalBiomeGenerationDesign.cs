using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeGenerationDesign :  BiomeGenerationDesign
    {
        public override bool HasRivers => true;
        public override bool HasPaths => true;
        public override bool HasDirt => true;
        
        private readonly FastNoiseSIMD _noise;

        public NormalBiomeGenerationDesign()
        {
            _noise = new FastNoiseSIMD(214121);
        }
        
        public override void BuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float Scale, Vector3 Offset)
        {
            var set = _noise.GetSimplexFractalSetWithFrequency(Offset, new Vector3(Width, Height, Width), new Vector3(Scale, Scale, Scale), 0.00075f);
            AddSet(DensityMap, set, F =>
            {
                if (F > 1)
                {
                    int a = 0;
                }
                return F.Clamp01() * 48.0f * Chunk.BlockSize;
            });

            AddFunction(
                DensityMap,
                SmallFrequency3DNoise
            );
        }

        private static float SmallFrequency3DNoise(int X, int Y, int Z)
        {
            return 0;//(World.GetNoise(X * 0.2f, Y * 0.2f, Z * 0.2f) * -0.15f * World.GetNoise(X * 0.035f, Y * 0.2f, Z * 0.035f) * 2.0f) * 7.5f;
        }

        public override void BuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            var set = _noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.0001f);
            AddSet(HeightMap, set, F => F * 16.0f);
            AddConstant(HeightMap, BiomePool.SeaLevel);
        }
    }
}
