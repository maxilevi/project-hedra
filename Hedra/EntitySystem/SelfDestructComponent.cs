using System;
using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;

namespace Hedra.EntitySystem
{
    public class SelfDestructComponent : EntityComponent
    {
        private readonly Func<bool> _condition;
        private bool _disposed;
        
        public SelfDestructComponent(IEntity Entity, Func<bool> When) : base(Entity)
        {
            _condition = When;
        }
        
        public SelfDestructComponent(IEntity Entity, float Time) : base(Entity)
        {
            var timer = new Timer(Time);
            _condition = () => timer.Tick();
        }

        public override void Update()
        {
             if(_condition())
                 Kill();
        }
        
        private void Kill()
        {
            if(_disposed) return;
            _disposed = true;
            Executer.ExecuteOnMainThread(() => Parent.Dispose());
        }
    }
}