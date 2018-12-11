using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class IdleBehaviour : Behaviour
    {
        protected StareBehaviour Stare { get; }

        public IdleBehaviour(IEntity Parent) : base(Parent)
        {
            Stare = new StareBehaviour(Parent);
        }

        public override void Update()
        {
            Stare.Update();
        }
    }
}
