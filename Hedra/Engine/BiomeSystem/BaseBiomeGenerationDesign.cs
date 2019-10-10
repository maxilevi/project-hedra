using System;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Native;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class BaseBiomeGenerationDesign : BiomeGenerationDesign
    {
        public const int RiverDepth = 6;
        public const int PathDepth = 2;
        public const int PathBorder = 64;

        protected override void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            BaseRiver(Noise, RiverMap, Width, Scale, Offset, 48f);
        }

        protected override void DoBuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
            BaseRiver(Noise, RiverMap, Width, Scale, Offset, 42f);
        }
        
        private static void BaseRiver(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset, float Border)
        {
            Noise.PerturbType = PerturbType.Gradient;
            Noise.PerturbFrequency = 32f;
            Noise.PerturbAmp = 0.0075f;
            
            Noise.Seed = World.Seed;
            var set1 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00015f);
            set1 = TransformSet(set1, F => (float) Math.Min(16f, Math.Max(0, 1.0 - Math.Abs(F) * Border)));
            
            Noise.Seed = World.Seed + 100;
            var set2 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00015f);
            set2 = TransformSet(set2, F => (float) Math.Min(16f, Math.Max(0, 1.0 - Math.Abs(F) * Border)));
            
            Noise.Seed = World.Seed + 2000;
            var set3 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00015f);
            set3 = TransformSet(set3, F => (float) Math.Min(16f, Math.Max(0, 1.0 - Math.Abs(F) * Border)));

            Noise.Seed = World.Seed + 30000;
            var set4 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00015f);
            set4 = TransformSet(set4, F => (float) Math.Min(16f, Math.Max(0, 1.0 - Math.Abs(F) * Border)));

            
            Noise.PerturbType = PerturbType.None;
            
            OperateSets(set1, set2, (F1, F2) => F1 + F2);
            OperateSets(set1, set3, (F1, F2) => F1 + F2);
            OperateSets(set1, set4, (F1, F2) => F1 + F2);
            AddSet(RiverMap, set1, F => Math.Min(RiverDepth, Math.Max(0, F) * 8f));
        }
        
        protected override void DoBuildPathMap(FastNoiseSIMD Noise, float[][] PathMap, int Width, float Scale, Vector2 Offset)
        {
            var set1 = Noise.GetSimplexSetWithFrequency(Offset + new Vector2(1000, 1000), new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00015f);
            set1 = TransformSet(set1, F => (float) Math.Min(16f, Math.Max(0, 1.0 - Math.Abs(F) * PathBorder)));

            AddSet(PathMap, set1, F => Math.Min(PathDepth, Math.Max(0, F) * 8f));
        }
    }
}