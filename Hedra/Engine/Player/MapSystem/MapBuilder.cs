using Hedra.Engine.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapBuilder
    {
        private static readonly CollidableStructure[] EmptyItems = new CollidableStructure[0];

        public StructureDesign Sample(Vector3 Position, Region Biome)
        {
            var chunkOffset = World.ToChunkSpace(Position);
            for (var i = 0; i < Biome.Structures.Designs.Length; i++)
            {
                var design = Biome.Structures.Designs[i];
                var rng = new RandomDistribution(StructureDesign.BuildRngSeed(chunkOffset));
                var targetPosition = StructureDesign.BuildTargetPosition(chunkOffset, rng);
                if (design.ShouldSetup(chunkOffset, targetPosition, EmptyItems, Biome, rng) && design.CanSetup(targetPosition))
                {
                    return design;
                }
            }
            return null;
        }
    }
}
