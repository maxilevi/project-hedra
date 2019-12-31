using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Structures
{
    public class MapBuilder
    {
        private static readonly CollidableStructure[] EmptyItems = new CollidableStructure[0];

        public static StructureDesign Sample(Vector3 Position, Region Biome)
        {
            var designAtPosition = World.StructureHandler.StructureItems.FirstOrDefault(C => C.MapPosition == World.ToChunkSpace(Position))?.Design;
            if (designAtPosition != null) return designAtPosition;
            var chunkOffset = World.ToChunkSpace(Position);
            var rng = new RandomDistribution();
            for (var i = 0; i < Biome.Structures.Designs.Length; i++)
            {
                var design = Biome.Structures.Designs[i];
                rng.Seed = StructureDesign.BuildRngSeed(chunkOffset);
                var targetPosition = StructureDesign.BuildTargetPosition(chunkOffset, rng);
                if (design.ShouldSetup(chunkOffset, ref targetPosition, EmptyItems, Biome, rng) && !design.InterferesWithAnotherStructure(targetPosition))
                {
                    return design;
                }
            }
            return null;
        }
    }
}
