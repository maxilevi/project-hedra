using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class SpawnGhostTownPortalDesign : GhostTownPortalDesign
    {
        public static bool Spawned { get; set; }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            return base.Setup(TargetPosition, Rng);
        }
        
        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
           return ChunkOffset == World.ToChunkSpace(World.SpawnPoint) && !Spawned;
        }
        
        protected override GhostTownPortal Create(Vector3 Position, float Size)
        {
            return new SpawnGhostTownPortal(Position, Scale);
        }
        
    }
}