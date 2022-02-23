using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using System.Numerics;

namespace HedraTests.Structure
{
    public class SimpleGenerationDesignMock : BaseBiomeGenerationDesign
    {
        private readonly Func<float> _returnValue;
        
        public SimpleGenerationDesignMock(Func<float> ReturnValue)
        {
            _returnValue = ReturnValue;
        }

        public override bool HasRivers { get; }
        
        public override bool HasPaths { get; }
        
        public override bool HasDirt { get; }

        protected override void DoBuildDensityMap(FastNoiseSIMD Noise, float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
        }

        protected override void DoBuildHeightMap(FastNoiseSIMD Noise, float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            HeightMap[0][0] = _returnValue();
        }
        
        protected override void DoBuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
        }

        protected override void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale, Vector2 Offset)
        {
        }
    }
}