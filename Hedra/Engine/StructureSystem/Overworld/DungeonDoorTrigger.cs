using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonDoorTrigger : BuildingDoorTrigger, IUpdatable
    {
        private readonly TimeHandler _handler;
        public DungeonDoorTrigger(Vector3 Position, VertexData Mesh) : base(Position, Mesh)
        {
            _handler = new TimeHandler(1000, SoundType.DarkSound);
            UpdateManager.Add(this);
        }

        protected override void OnEnter(IEntity Entity)
        {
            if (Entity is LocalPlayer)
                Apply();
        }

        protected override void OnLeave(IEntity Entity)
        {
            if (Entity is LocalPlayer)
                Reset();
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