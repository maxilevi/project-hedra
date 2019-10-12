using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.Generation;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.BiomeSystem.GhostTown
{
    public class GhostTownGenerationDesign : LegacyBiomeGenerator
    {
        public const float IslandRadius = 2048;
        public override bool HasPaths => false;
        public override bool HasRivers => true;
        public override bool HasDirt => false;

        protected override float GetHeight(float X, float Z, out BlockType Blocktype)
        {
            var height = 0.0;
            Blocktype = BlockType.Air;

            AddBaseHeight(X, Z, ref height, ref Blocktype, out _);
            AddStones(X, Z, ref height, ref Blocktype);

            var multiplier = HeightMultiplier(new Vector2(X, Z));
            return ((float)height * 1.5f * multiplier) + (BiomeGenerator.SmallFrequency(X, Z) * 1.5f);
        }
        
        protected static void AddBaseHeight(float X, float Z, ref double Height, ref BlockType Type, out double BaseHeight)
        {
            const int baseHeight = BiomePool.SeaLevel;
            var grassHeight = (World.GetNoise(X * 0.004f, Z * 0.004f) + .25f) * 3.0f;
            Height += baseHeight + grassHeight;
            BaseHeight = baseHeight;
            Type = BlockType.Grass;
        }

        private static float HeightMultiplier(Vector2 Position)
        {
            return 1f - Math.Max(0, Math.Min(1, (Position - World.SpawnPoint.Xz).LengthFast / IslandRadius ));
        }
        
        private static void AddStones(float X, float Z, ref double Height, ref BlockType Type)
        {
            var stones = Math.Max(0, World.GetNoise(X * 0.005f, Z * 0.005f) - .5) * 48.0;
            Height += stones;
            if (stones > 0)
            {
                Height += BiomeGenerator.SmallFrequency(X + 234, Z + 12123) * 2.0;
                Type = BlockType.Stone;
            }
        }
    }
}