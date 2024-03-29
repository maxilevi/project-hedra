using System.Numerics;
using Hedra.Engine.Generation;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class LegacyBiomeGenerator : BaseBiomeGenerationDesign
    {
        protected override void DoBuildDensityMap(FastNoiseSIMD Noise, float[][][] DensityMap, BlockType[][][] TypeMap,
            int Width, int Height, float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
        }

        protected override void DoBuildHeightMap(FastNoiseSIMD Noise, float[][] HeightMap, BlockType[][] TypeMap,
            int Width, float Scale, Vector2 Offset)
        {
            for (var x = 0; x < Width; x++)
            for (var z = 0; z < Width; z++)
                HeightMap[x][z] = GetHeight(x * Scale + Offset.X, z * Scale + Offset.Y, out _);
        }

        protected abstract float GetHeight(float X, float Z, out BlockType BlockType);
    }
}