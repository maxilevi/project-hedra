using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem.NormalBiome;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.GhostTown
{
    public class GhostTownGenerationDesign : NormalBiomeGenerationDesign
    {
        public const float IslandRadius = 2048;
        public override bool HasPaths => false;
        public override bool HasRivers => true;
        public override bool HasDirt => false;

        public override float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache, out BlockType Blocktype)
        {
            var height = 0.0;
            Blocktype = BlockType.Air;

            AddBaseHeight(X, Z, ref height, ref Blocktype, out _);
            AddStones(X, Z, ref height, ref Blocktype, HeightCache);

            var multiplier = HeightMultiplier(new Vector2(X, Z));
            return ((float)height * 1.5f * multiplier) + (BiomeGenerator.SmallFrequency(X, Z) * 1.5f);
        }

        private static float HeightMultiplier(Vector2 Position)
        {
            return 1f - Math.Max(0, Math.Min(1, (Position - World.SpawnPoint.Xz).LengthFast / IslandRadius ));
        }
    }
}