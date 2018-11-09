using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class TrollAIComponent : BasicAIComponent
    {
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public TrollAIComponent(IEntity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
            this.AlterBehaviour<AttackBehaviour>(new TrollAttackBehaviour(Entity));
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
    }
}