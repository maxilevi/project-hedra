using System;
using System.Collections.Generic;
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
        
        public override bool HasHeightSubtype(float X, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            throw new NotImplementedException();
        }
        
        public override bool HasRivers { get; set; }
        
        public override bool HasPaths { get; set; }
        
        public override bool HasDirt { get; set; }
        
        public override float GetDensity(float X, float Y, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            return _returnValue();
        }

        public override BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            throw new System.NotImplementedException();
        }

        public override float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache, out BlockType Blocktype)
        {
            Blocktype = BlockType.Grass;
            return _returnValue();
        }
    }
}