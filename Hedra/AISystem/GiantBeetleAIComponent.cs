using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem
{
    public class GiantBeetleAIComponent : BasicAIComponent, IGuardAIComponent
    {
        protected RetaliateBehaviour Retaliate { get; }
        protected GuardHostileBehaviour Hostile { get; }

        public GiantBeetleAIComponent(IEntity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new GuardHostileBehaviour(Parent);
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

        public Vector3 GuardPosition
        {
            get => Hostile.GuardPosition;
            set => Hostile.GuardPosition = value;
        }
        
        public override AIType Type => AIType.Hostile;
    }
}
