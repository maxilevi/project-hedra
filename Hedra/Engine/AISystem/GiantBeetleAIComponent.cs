using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.AISystem
{
    public class GiantBeetleAIComponent : BasicAIComponent
    {
        protected RetaliateBehaviour Retaliate { get; }
        protected HostileBehaviour Hostile { get; }

        public GiantBeetleAIComponent(IEntity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new HostileBehaviour(Parent);
            this.AlterBehaviour<AttackBehaviour>(new GiantBeetleAttackBehaviour(Entity));
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
