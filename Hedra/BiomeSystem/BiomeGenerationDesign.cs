using System.Collections.Generic;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeGenerationDesign
    {
        public abstract bool HasRivers { get; set; }
        public abstract bool HasPaths { get; set; }
        public abstract bool HasDirt { get; set; }

        public abstract float GetDensity(float X, float Y, float Z, ref BlockType Type);

        public abstract BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type,
            Dictionary<Vector2, float[]> HeightCache);
        
        public abstract bool HasHeightSubtype(float X, float Z, Dictionary<Vector2, float[]> HeightCache);

        public abstract float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache,
            out BlockType Blocktype);
    }
}
