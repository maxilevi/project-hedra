using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class LegacyBiomeGenerator : BiomeGenerationDesign
    {
        public override void BuildDensityMap(float[][][] DensityMap, BlockType[][][] TypeMap, int Width, int Height, float Scale, Vector3 Offset)
        {
        }

        public override void BuildHeightMap(float[][] HeightMap, BlockType[][] TypeMap, int Width, float Scale, Vector2 Offset)
        {
            for (var x = 0; x < Width; x++)
            {
                for(var z = 0; z < Width; z++)
                {
                    HeightMap[x][z] = GetHeight(x * Scale + Offset.X, z * Scale + Offset.Y, out TypeMap[x][z]);
                }
            }
        }

        protected abstract float GetHeight(float X, float Z, out BlockType BlockType);
    }
}