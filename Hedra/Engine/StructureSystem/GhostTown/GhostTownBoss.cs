using System.Numerics;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Structures;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownBoss : BaseStructure, ICompletableStructure
    {
        public GhostTownBoss(Vector3 Position) : base(Position)
        {
        }

        public IEntity Boss { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            Boss?.Dispose();
            GhostTownBossDesign.Spawned = false;
        }

        public bool Completed => Boss.IsDead;
    }
}