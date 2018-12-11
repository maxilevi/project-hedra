using System;
using Hedra.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class SpawnCampfireDesign : CampfireDesign
    {    
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var realPosition = World.SpawnPoint;
            World.StructureHandler.SpawnCampfireSpawned = true;
            var structure = base.Setup(realPosition, Rng, new SpawnCampfire(realPosition));
            structure.Mountain.Radius = 48;
            return structure;
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnPoint) && !World.StructureHandler.SpawnCampfireSpawned;
        }
    }
}