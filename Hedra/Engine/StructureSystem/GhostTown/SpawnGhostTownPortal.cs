using System.Numerics;
using Hedra.Engine.Player;
using Hedra.Structures;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class SpawnGhostTownPortal : GhostTownPortal
    {
        public SpawnGhostTownPortal(Vector3 Position, Vector3 Scale)
            : base(Position, Scale, RealmHandler.Overworld)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            SpawnGhostTownPortalDesign.Spawned = false;
        }
    }
}