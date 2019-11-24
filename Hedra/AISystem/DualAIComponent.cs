using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public class DualAIComponent : Component<IEntity>
    {
        private readonly IComponent<IEntity> _ai1;
        private readonly IComponent<IEntity> _ai2;
        private IComponent<IEntity> _current;
        
        public DualAIComponent(IEntity Parent, IComponent<IEntity> AI1, IComponent<IEntity> AI2) : base(Parent)
        {
            _ai1 = AI1;
            _ai2 = AI2;
            _current = AI1;
        }

        public override void Update()
        {
            _current.Update();
        }

        public void Switch()
        {
            _current = _ai1 == _current 
                ? _ai2 
                : _ai1;
        }
        
        public void SwitchOne()
        {
            _current = _ai1;
        }
        
        public void SwitchTwo()
        {
            _current = _ai2;
        }

        public override void Draw()
        {
            base.Draw();
            _current.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _ai1.Dispose();
            _ai2.Dispose();
        }
    }
}