using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem.GhostTown;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Structures
{
    public class SpawnGhostTownPortalDesign : GhostTownPortalDesign
    {
        public override int PlateauRadius => 180;
        protected override bool SpawnNPC => false;
        public static bool Spawned { get; set; }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            return base.Setup(TargetPosition, Rng);
        }
        
        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
           return ChunkOffset == World.ToChunkSpace(Position) && !Spawned;
        }
        
        protected override GhostTownPortal Create(Vector3 TargetPosition, float Size)
        {
            return new SpawnGhostTownPortal(Position, Scale);
        }

        public static Vector3 Position => World.SpawnPoint + GhostTownGenerationDesign.IslandRadius * -Vector3.One * .25f;
    }
}