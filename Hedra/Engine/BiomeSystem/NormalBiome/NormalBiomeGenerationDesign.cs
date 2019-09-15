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
        
        public override void BuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            var offset = Offset;
            var size = new Vector3(Width, Height, Width);
            var scale = new Vector3(HorizontalScale, VerticalScale, HorizontalScale);
            /* Set 1, generates small mountains with some overhangs */
            
            var set1 = Noise.GetSimplexFractalSetWithFrequency(offset, size, scale, 0.0005f);
            set1 = MultiplySets(set1, Noise.GetSimplexFractalSetWithFrequency(offset, size, scale, 0.001f));
            AddSet(DensityMap, set1, F => F.Clamp01() * 48.0f * Chunk.BlockSize);
            
            
            var set = Noise.GetSimplexFractalSetWithFrequency(offset, size, scale, 0.00075f);
            set = MultiplySets(
                set,
                Noise.GetSimplexFractalSetWithFrequency(offset + new Vector3(1000, 1000, 1000), size, scale, 0.00075f)
            );
            AddSet(DensityMap, set, F => (F).Clamp01() * 96.0f * Chunk.BlockSize);
            
            /* Small set, generate high frequency noise to avoid making the terrain soft */
            var smallSet = MultiplySets(
                Noise.GetSimplexSetWithFrequency(offset, size, scale, 0.2f),
                Noise.GetSimplexSetWithFrequency(offset, size, scale, 0.075f)
            );
            AddSet(DensityMap, smallSet, F => F * -1.0f);
        }

        public override void BuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            var set = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.0001f);
            AddSet(HeightMap, set, F => F * 16.0f);
            AddConstant(HeightMap, BiomePool.SeaLevel);
        }

        public void BuildRiverMap(float[][] Map, Vector2 Offset, int Width, float Scale, float Narrow, float Border, float RiverScale)
        {
            var set = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.001f);
            set = TransformSet(set, F => (float) Math.Max(0, 0.5 - Math.Abs(F - 0.2) - Narrow + Border) * Scale);
            AddSet(Map, set, F => F);
        }
    }
}
