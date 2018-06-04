using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public class GorillaWarriorAIComponent : BasicAIComponent
    {
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public GorillaWarriorAIComponent(Entity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
            this.AlterBehaviour<AttackBehaviour>(new GorillaWarriorAttackBehaviour(Entity));
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
