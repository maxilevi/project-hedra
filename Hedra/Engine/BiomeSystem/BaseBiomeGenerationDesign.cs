using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Native;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class BaseBiomeGenerationDesign : BiomeGenerationDesign
    {
        public const int RiverDepth = 4;
        public const float PathDepth = 2.5f;
        public const int PathBorder = 64;

        protected override void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale,
            Vector2 Offset)
        {
            BaseRiver(Noise, RiverMap, Width, Scale, Offset, 38f);
        }

        protected override void DoBuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale,
            Vector2 Offset)
        {
            BaseRiver(Noise, RiverMap, Width, Scale, Offset, 32f);
        }

        private static Func<Vector<float>, Vector<float>> BindRiverTransformFunction(float Border)
        {
            Vector<float> TransformFunc(Vector<float> F) => Vector.Min(new Vector<float>(16f),
                Vector.Max(Vector<float>.Zero, Vector<float>.One - Vector.Abs(F) * Border));

            return TransformFunc;
        }


        private static void BaseRiver(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset,
            float Border)
        {
            Noise.PerturbType = PerturbType.Gradient;
            Noise.PerturbFrequency = 32f;
            Noise.PerturbAmp = 0.0075f;
            
            Noise.Seed = World.Seed;
            var set1 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                0.00015f);
            set1 = TransformSet(set1, BindRiverTransformFunction(Border));

            Noise.Seed = World.Seed + 100;
            var set2 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                0.00015f);
            set2 = TransformSet(set2, BindRiverTransformFunction(Border));

            Noise.Seed = World.Seed + 2000;
            var set3 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                0.00015f);
            set3 = TransformSet(set3, BindRiverTransformFunction(Border));

            Noise.Seed = World.Seed + 30000;
            var set4 = Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                0.00015f);
            set4 = TransformSet(set4, BindRiverTransformFunction(Border));


            Noise.PerturbType = PerturbType.None;

            OperateSets(set1, set2, (F1, F2) => F1 + F2);
            OperateSets(set1, set3, (F1, F2) => F1 + F2);
            OperateSets(set1, set4, (F1, F2) => F1 + F2);
            AddSet(RiverMap, set1, F => Math.Min(RiverDepth, Math.Max(0, F) * 4f));
        }

        protected override void DoBuildPathMap(FastNoiseSIMD Noise, float[][] PathMap, int Width, float Scale,
            Vector2 Offset)
        {
            var set1 = Noise.GetSimplexSetWithFrequency(Offset + new Vector2(1000, 1000), new Vector2(Width, Width),
                new Vector2(Scale, Scale), 0.00015f);
            set1 = TransformSet(set1, BindRiverTransformFunction(PathBorder));

            /* Try to avoid increase this otherwise mountains with paths will look very sharp */
            AddSet(PathMap, set1, F => Math.Min(PathDepth, Math.Max(0, F) * 5f));
        }
    }
}