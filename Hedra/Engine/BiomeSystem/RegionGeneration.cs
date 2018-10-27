using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionGeneration
    {
        private readonly BiomeGenerationDesign _design;
        private readonly int _seed;

        public RegionGeneration(int Seed, BiomeGenerationDesign Design)
        {
            this._seed = Seed;
            this._design = Design;
        }

        public bool HasDirt => _design.HasDirt;
        public bool HasRivers => _design.HasRivers;
        public bool HasPaths => _design.HasPaths;

        public float GetDensity(float X, float Y, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            return _design.GetDensity(X, Y, Z, HeightCache);
        }

        public BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type,
            Dictionary<Vector2, float[]> HeightCache)
        {
            return _design.GetHeightSubtype(X, Y, Z, CurrentHeight, Type, HeightCache);
        }
        
        public bool HasHeightSubtype(float X, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            return _design.HasHeightSubtype(X, Z, HeightCache);
        }

        public float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache,
            out BlockType Blocktype)
        {
            return _design.GetHeight(X, Z, HeightCache, out Blocktype);
        }

        public static RegionGeneration Interpolate(params RegionGeneration[] RegionsGenerations)
        {
            //TODO Implement a good interpolation
            return RegionsGenerations[0];
        }
    }
}
