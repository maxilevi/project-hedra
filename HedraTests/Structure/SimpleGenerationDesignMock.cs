using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using OpenTK;

namespace HedraTests.Structure
{
    public class SimpleGenerationDesignMock : BiomeGenerationDesign
    {
        private readonly Func<float> _returnValue;
        
        public SimpleGenerationDesignMock(Func<float> ReturnValue)
        {
            _returnValue = ReturnValue;
        }

        public override bool HasRivers { get; }
        
        public override bool HasPaths { get; }
        
        public override bool HasDirt { get; }

        public override void DoBuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            throw new NotImplementedException();
        }

        public override void DoBuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            TypeMap[0][0] = BlockType.Grass;
            HeightMap[0][0] = _returnValue();
        }
    }
}