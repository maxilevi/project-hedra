using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonWithBoss : BaseStructure, ICompletableStructure, IStructureWithRadius, IUpdatable
    {
        private readonly StructureAmbientHandler _ambientHandler;
        
        public DungeonWithBoss(Vector3 Position, float Size, bool HasAmbientHandler) : base(Position)
        {
            GameManager.Player.StructureAware.StructureLeave += OnLeave;
            Radius = Size;
            if (HasAmbientHandler)
            {
                _ambientHandler = new StructureAmbientHandler(this, 1000);
                UpdateManager.Add(this);
            }
        }

        public IEntity Boss { get; set; }
        public float Radius { get; }
        public DungeonDoorTrigger BuildingTrigger { get; set; }

        public bool Completed => Boss != null && Boss.IsDead;

        public override void Dispose()
        {
            base.Dispose();
            GameManager.Player.StructureAware.StructureLeave -= OnLeave;
            if (_ambientHandler != null)
            {
                UpdateManager.Remove(this);
            }
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

        public void Update()
        {
            _ambientHandler.Update();
        }
    }
}