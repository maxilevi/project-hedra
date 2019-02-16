using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class DisposeComponent : EntityComponent
    {
        private readonly float _maxRadius;
        private bool _disposed;
        
        public DisposeComponent(IEntity Entity, float MaxRadius) : base(Entity)
        {
            _maxRadius = MaxRadius;
        }

        public override void Update()
        {
            if (!_disposed && GameManager.NearAnyPlayer(Parent.BlockPosition, _maxRadius))
                Kill();
        }

        private void Kill()
        {
            _disposed = true;
            Executer.ExecuteOnMainThread(() => Parent.Dispose());
        }
    }
}