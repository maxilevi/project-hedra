using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0 : BaseStructure, ICompletableStructure
    {
        public IEntity Boss { get; set; }
        public DungeonDoorTrigger BuildingTrigger { get; set; }

        public Dungeon0(Vector3 Position) : base(Position)
        {
            GameManager.Player.StructureAware.StructureLeave += OnLeave;
        }

        private void OnLeave(CollidableStructure Structure)
        {
            Reset();
        }

        private void Reset()
        {
            if(BuildingTrigger != null)
                BuildingTrigger.Leave(GameManager.Player);
            if(Boss != null)
                Boss.Health = Boss.MaxHealth;
        }

        public bool Completed => Boss.IsDead;

        public override void Dispose()
        {
            base.Dispose();
            GameManager.Player.StructureAware.StructureLeave -= OnLeave;
        }
    }
}