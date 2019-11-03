using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class TrollAIComponent : BasicAIComponent, IGuardAIComponent
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
        
        public override AIType Type => AIType.Hostile;
        public Vector3 GuardPosition { get; set; }
    }
}