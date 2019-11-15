using System.Numerics;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonBossRoomTrigger : CollisionTrigger
    {
        public IEntity Boss { get; set; }
        private bool _entered;
        public DungeonBossRoomTrigger(Vector3 Position, VertexData Mesh) : base(Position, Mesh)
        {
            OnCollision += OnEnter;
        }

        private void OnEnter(IEntity Entity)
        {
            if(Entity != LocalPlayer.Instance || _entered) return;
            _entered = true;
            Boss.SearchComponent<BossHealthBarComponent>().Enabled = true;
        }
    }
}