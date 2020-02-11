using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public abstract class BaseBeetleAIComponent : BasicAIComponent
    {
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        protected BaseBeetleAIComponent(IEntity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = GetHostileBehaviour(Parent);
            AlterBehaviour(GetAttackBehaviour(Entity));
        }

        public override void Update()
        {
            if (Retaliate.Enabled)
            {
                Retaliate.Update();
            }
            else
            {
                Hostile.Update();
            }
        }

        protected virtual HostileBehaviour GetHostileBehaviour(IEntity Parent)
        {
            return new HostileBehaviour(Parent);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            Hostile.Dispose();
            Retaliate.Dispose();
        }

        protected abstract AttackBehaviour GetAttackBehaviour(IEntity Parent);

        public override AIType Type => AIType.Hostile;
    }
}