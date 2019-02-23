using Hedra.Engine.Management;
using Hedra.Engine.EntitySystem;

namespace Hedra.EntitySystem
{
    public class SelfDestructComponent : EntityComponent
    {
        private readonly Timer _timer;
        private bool _disposed;
        
        public SelfDestructComponent(IEntity Entity, float Time) : base(Entity)
        {
            _timer = new Timer(Time);
        }

        public override void Update()
        {
             if(_timer.Tick())
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