using Facepunch.Steamworks;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Structures;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.GhostTown
{
    public class GhostTownBoss : BaseStructure, ICompletableStructure
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

        public bool Completed => Boss.IsDead;
    }
}