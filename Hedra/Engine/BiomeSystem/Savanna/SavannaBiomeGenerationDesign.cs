using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;

namespace Hedra.Engine.BiomeSystem.Savanna
{
    public class SavannaBiomeGenerationDesign : BiomeGenerationDesign
    {
        public override bool HasRivers { get; }
        public override bool HasPaths { get; }
        public override bool HasDirt { get; }

        protected override void DoBuildDensityMap(FastNoiseSIMD Noise, float[][][] DensityMap, BlockType[][][] TypeMap,
            int Width, int Height,
            float HorizontalScale, float VerticalScale, Vector3 Offset)
        {
            throw new NotImplementedException();
        }

        protected override void DoBuildHeightMap(FastNoiseSIMD Noise, float[][] HeightMap, BlockType[][] TypeMap,
            int Width, float Scale,
            Vector2 Offset)
        {
            throw new NotImplementedException();
        }

        protected override void DoBuildRiverMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale,
            Vector2 Offset)
        {
            throw new NotImplementedException();
        }

        protected override void DoBuildRiverBorderMap(FastNoiseSIMD Noise, float[][] RiverMap, int Width, float Scale,
            Vector2 Offset)
        {
            throw new NotImplementedException();
        }

        protected override void DoBuildPathMap(FastNoiseSIMD Noise, float[][] PathMap, int Width, float Scale,
            Vector2 Offset)
        {
            throw new NotImplementedException();
        }

        protected override void DoBuildLandformMap(FastNoiseSIMD Noise, float[][] LandformMap, int Width, float Scale, Vector2 Offset)
        {
            throw new NotImplementedException();
        }
    }
}