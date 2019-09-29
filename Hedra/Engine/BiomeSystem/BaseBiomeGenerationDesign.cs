using System;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Native;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class BaseBiomeGenerationDesign : BiomeGenerationDesign
    {
        private const float Narrow = 0.42f;
        private const float Border = 0.02f;
        public const int RiverDepth = 6;

        protected override void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            BaseRiver(Noise, RiverMap, Width, Scale, Offset, 48f);
        }

        protected override void DoBuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            BaseRiver(Noise, RiverMap, Width, Scale, Offset, 42f);
        }
        
        private void BaseRiver(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset, float Border)
        {
            var set1 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00015f);
            set1 = TransformSet(set1, F => (float) Math.Min(16f, Math.Max(0, 1.0 - Math.Abs(F) * Border)));
            
            var set2 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.001f);
            set2 = TransformSet(set2, F => Math.Min(1f, Math.Max(0f, F)));
            //set1 = MultiplySets(set1, set2);
            
            AddSet(RiverMap, set1, F => Math.Min(RiverDepth, Math.Max(0, F) * 8f));
        }
    }
}