using System;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class BaseBiomeGenerationDesign : BiomeGenerationDesign
    {
        private const float Narrow = 0.42f;
        private const float Border = 0.02f;

        protected override void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            var set = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.001f);
            set = TransformSet(set, F => (float) Math.Max(0, 0.5 - Math.Abs(F - 0.2) - Narrow + Border) * Scale);
            AddSet(RiverMap, set, F => F);
        }
    }
}