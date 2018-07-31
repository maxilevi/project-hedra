using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class IdleBehaviour : Behaviour
    {
        protected StareBehaviour Stare { get; }

        public IdleBehaviour(Entity Parent) : base(Parent)
        {
            Stare = new StareBehaviour(Parent);
        }

        public override void Update()
        {
            Stare.Update();
            Parent.Model.Idle();
        }
    }
}
