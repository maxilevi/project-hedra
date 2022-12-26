using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Numerics;

namespace Hedra.Engine.BiomeSystem.ShroomDimension
{

    public class ShroomDimensionGenerationDesign : BaseBiomeGenerationDesign
    {
        public const float IslandRadius = 2048 * 4;
        public override bool HasRivers => false;
        public override bool HasPaths => false;
        public override bool HasDirt => false;
        
        public override float DirtFrequency => 0.0001f;
        public override float DirtThreshold => 0.5f;
        
        private static float HeightMultiplier(Vector2 Position)
        {
            return 1f - Math.Max(0, Math.Min(1, (Position - World.SpawnPoint.Xz()).LengthFast() / IslandRadius));
        }

        protected override void DoBuildDensityMap(FastNoiseSIMD Noise, float[][][] DensityMap, BlockType[][][] TypeMap,
            int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            var offset = Offset;
            var size = new Vector3(Width, Height, Width);
            var scale = new Vector3(HorizontalScale, VerticalScale, HorizontalScale);

            /* Medium hangs and mountains, sparse. */
            var set1 = Noise.GetPerlinSetWithFrequency(offset, size, scale, 0.0015f);
            set1 = MultiplySets(
                set1,
                TransformSet(Noise.GetSimplexSetWithFrequency(Offset, size, scale, 0.00025f), F => F)
            );
            set1 = MultiplySetsDimensional(
                set1,
                TransformSet(Noise.GetSimplexSetWithFrequency(Offset.Xz(), size.Xz(), scale.Xz(), 0.0001f),
                    F => (F - new Vector<float>(0.05f)).Clamp01() * 2),
                size
            );
            AddSet(DensityMap, set1, F => ((F - new Vector<float>(-0.005f)) * 2.5f) * 64.0f * Chunk.BlockSize);

            var set = Noise.GetPerlinFractalSetWithFrequency(offset, size, scale, 0.00015f);
            set = MultiplySets(
                set,
                TransformSet(Noise.GetPerlinSetWithFrequency(offset, size, scale, 0.00175f), F => F)
            );
            set = MultiplySetsDimensional(
                set,
                TransformSet(Noise.GetSimplexSetWithFrequency(Offset.Xz(), size.Xz(), scale.Xz(), 0.00045f),
                    F => (F + new Vector<float>(0.35f)).Clamp01() * 3f),
                size
            );
            AddSet(DensityMap, set, F => F * 256.0f * Chunk.BlockSize);

            /* Small set, generate high frequency noise to avoid making the terrain smooth */
            var smallSet = MultiplySets(
                Noise.GetSimplexSetWithFrequency(offset, size, scale, 0.2f),
                Noise.GetSimplexSetWithFrequency(offset, size, scale, 0.075f)
            );
            AddSet(DensityMap, smallSet, F => F * -1.0f);
        }

        protected override void DoBuildHeightMap(FastNoiseSIMD Noise, float[][] HeightMap, BlockType[][] TypeMap,
            int Width, float Scale, Vector2 Offset)
        {
            var baseSet = Noise.GetPerlinSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                0.000025f);
            AddSet(HeightMap, baseSet, F => (F + 0.25f).Clamp01() * 32.0f);

            var bigMountain =
                Noise.GetPerlinSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.00025f);
            AddSet(HeightMap, bigMountain, F => F.Clamp01() * 256.0f);

            var lakeSet = Noise.GetPerlinSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                0.00025f);
            AddSet(HeightMap, lakeSet, F => (F - 0.2f).Clamp01() * -128.0f);

            /* Sparse lakes */
            var sparseLakeSet = Noise.GetPerlinSetWithFrequency(Offset, new Vector2(Width, Width),
                new Vector2(Scale, Scale), 0.0005f);
            sparseLakeSet = MultiplySets(
                sparseLakeSet,
                TransformSet(
                    Noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale),
                        0.00075f), F => F)
            );
            //AddSet(HeightMap, sparseLakeSet, F => F.Clamp01() * -128.0f);

            AddConstant(HeightMap, BiomePool.SeaLevel);
            for (var x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    HeightMap[x][y] *= HeightMultiplier(Offset + new Vector2(x, y));
                }
            }
        }
        
    }
}