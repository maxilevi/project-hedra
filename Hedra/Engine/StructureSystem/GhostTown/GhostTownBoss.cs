using Facepunch.Steamworks;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownBoss : BaseStructure
    {
        public IEntity Boss { get; set; }
        public GhostTownBoss(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Boss?.Dispose();
            GhostTownBossDesign.Spawned = false;
        }
    }
}