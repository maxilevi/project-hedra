using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonTimeTrigger : CollisionTrigger, IUpdatable
    {
        public bool IsPlayerInside => _handler.IsActive;
        private readonly TimeHandler _handler;
        public DungeonTimeTrigger(Vector3 Position, VertexData Mesh) : base(Position, Mesh)
        {
            _handler = new TimeHandler(0);
            OnCollision += E =>
            {
                if (E == LocalPlayer.Instance)
                {
                    if (!IsPlayerInside)
                        Apply();
                    else
                        Reset();
                }
            };
            UpdateManager.Add(this);
        }

        public void Apply()
        {
            _handler.Apply();
            GameSettings.DepthEffect = true;
        }
        
        public void Reset()
        {
            _handler.Remove();
            GameSettings.DepthEffect = false;
        }
        
        public void Update()
        {
            _handler.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
            _handler.Dispose();
        }
    }
}