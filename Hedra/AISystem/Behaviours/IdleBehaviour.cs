using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class IdleBehaviour : Behaviour
    {
        public IdleBehaviour(IEntity Parent) : base(Parent)
        {
            Stare = new StareBehaviour(Parent);
        }

        protected StareBehaviour Stare { get; }

        public override void Update()
        {
            Stare.Update();
        }

        public override void Dispose()
        {
            Stare.Dispose();
        }
    }
}