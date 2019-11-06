using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0Trigger : CollisionTrigger, IUpdatable
    {
        private readonly TimeHandler _handler;
        public Dungeon0Trigger(Vector3 Position, VertexData Mesh) : base(Position, Mesh)
        {
            _handler = new TimeHandler(0);
            OnCollision += E =>
            {
                if (E != LocalPlayer.Instance) return;
                if(!_handler.IsActive)
                    _handler.Apply();
                else
                    _handler.Remove();
            };
            UpdateManager.Add(this);
        }

        public void Update()
        {
            _handler.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
            _handler.Dispose();
        }
    }
}