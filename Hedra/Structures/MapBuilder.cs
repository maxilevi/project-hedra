using Hedra.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using System.Numerics;

namespace Hedra.Structures
{
    public class MapBuilder
    {
        private static readonly CollidableStructure[] EmptyItems = new CollidableStructure[0];

        public static StructureDesign Sample(Vector3 Position, Region Biome)
        {
            var chunkOffset = World.ToChunkSpace(Position);
            var rng = new RandomDistribution();
            for (var i = 0; i < Biome.Structures.Designs.Length; i++)
            {
                var design = Biome.Structures.Designs[i];
                rng.Seed = StructureDesign.BuildRngSeed(chunkOffset);
                var targetPosition = StructureDesign.BuildTargetPosition(chunkOffset, rng);
                if (design.ShouldSetup(chunkOffset, targetPosition, EmptyItems, Biome, rng))
                {
                    return design;
                }
            }
            return null;
        }
    }
}
