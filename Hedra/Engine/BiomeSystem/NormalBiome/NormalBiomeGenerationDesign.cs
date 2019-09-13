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
        
        public override void BuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            var offset = Offset;
            var size = new Vector3(Width, Height, Width);
            var scale = new Vector3(HorizontalScale, VerticalScale, HorizontalScale);
            var set = _noise.GetSimplexFractalSetWithFrequency(offset, size, scale, 0.00075f);
            AddSet(DensityMap, set, F => F.Clamp01() * 48.0f * Chunk.BlockSize);

            var smallSet = MultiplySets(
                _noise.GetSimplexSetWithFrequency(offset, size, scale, 0.2f),
                _noise.GetSimplexSetWithFrequency(offset, size, scale, 0.075f)
            );
            AddSet(DensityMap, smallSet, F => F * -1.0f);
        }

        public override void BuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            var set = _noise.GetSimplexSetWithFrequency(Offset, new Vector2(Width, Width), new Vector2(Scale, Scale), 0.0001f);
            AddSet(HeightMap, set, F => F * 16.0f);
            AddConstant(HeightMap, BiomePool.SeaLevel);
        }
    }
}
