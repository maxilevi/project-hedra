using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonWithBoss : BaseStructure, ICompletableStructure
    {
        public DungeonWithBoss(Vector3 Position) : base(Position)
        {
            GameManager.Player.StructureAware.StructureLeave += OnLeave;
        }

        public IEntity Boss { get; set; }
        public DungeonDoorTrigger BuildingTrigger { get; set; }

        public bool Completed => Boss.IsDead;

        public override void Dispose()
        {
            base.Dispose();
            GameManager.Player.StructureAware.StructureLeave -= OnLeave;
        }

        private void OnLeave(CollidableStructure Structure)
        {
            Reset();
        }

        private void Reset()
        {
            if (BuildingTrigger != null)
                BuildingTrigger.Leave(GameManager.Player);
            if (Boss != null)
                Boss.Health = Boss.MaxHealth;
        }
    }
}