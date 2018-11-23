using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem.BossSystem
{
    /// <summary>
    /// Component that handles spawning on places where the chunks don't exist yet.
    /// </summary>
    public class SpawnComponent : EntityComponent, ITickable
    {
        private readonly Vector3 _position;
        private readonly Action _callback;
        
        public SpawnComponent(IEntity Entity, Vector3 Position, Action Callback) : base(Entity)
        {
            _position = Position;
            _callback = Callback;
        }

        public override void Update()
        {
            var underChunk = World.GetChunkAt(_position);
            if (underChunk == null) return;
            Parent.Position = new Vector3(_position.X, Physics.HeightAtPosition(_position), _position.Z);
            _callback?.Invoke();
            Parent.RemoveComponent(this);
            Dispose();
        }
    }
}