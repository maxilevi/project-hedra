using Hedra.Engine.Game;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class DisposeComponent : EntityComponent
    {
        private readonly float _maxRadius;
        
        public DisposeComponent(IEntity Entity, float MaxRadius) : base(Entity)
        {
            _maxRadius = MaxRadius;
        }

        public override void Update()
        {
            if (GameManager.NearAnyPlayer(Parent.BlockPosition, _maxRadius))
                Parent.Dispose();
        }
    }
}