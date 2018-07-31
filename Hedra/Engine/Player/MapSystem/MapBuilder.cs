using Hedra.Engine.BiomeSystem;
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
                var rng = design.BuildRng(chunkOffset);
                if (design.ShouldSetup(chunkOffset, design.BuildTargetPosition(chunkOffset, rng), EmptyItems, Biome, rng))
                    return design;
            }
            return null;
        }
    }
}
