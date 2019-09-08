using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.DesertBiome
{
    public class DesertBiomeGenerationDesign :  BiomeGenerationDesign
    {
        public override bool HasRivers { get; set; } = true;
        public override bool HasPaths { get; set; } = true;
        public override bool HasDirt { get; set; } = true;

        public override bool HasHeightSubtype(float X, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            throw new NotImplementedException();
        }

        public override float GetDensity(float X, float Y, float Z, ref BlockType Type)
        {
            throw new NotImplementedException();
        }

        public override BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            if (HeightCache.ContainsKey(new Vector2(X, Z)))
            {
                double height = HeightCache[new Vector2(X, Z)][1];
                double realHeight = (CurrentHeight - height) / HeightCache[new Vector2(X, Z)][2];

                if (Math.Abs(realHeight - 32.0) < 0.5f)
                {
                    if (Y > 28.0)
                        return BlockType.Grass;
                }
                return Type;
            }
            return Type;
        }

        public override float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache,
            out BlockType Blocktype)
        {
            Blocktype = BlockType.Stone;
            return 0;
        }
    }
}
