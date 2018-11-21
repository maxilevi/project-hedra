using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using OpenTK;

namespace HedraContent.AI
{
    public class GorillaWarriorAIComponent : BasicAIComponent, IGuardAIComponent
    {
        protected RetaliateBehaviour Retaliate { get; }
        protected GuardHostileBehaviour Hostile { get; }

        public GorillaWarriorAIComponent(Entity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new GuardHostileBehaviour(Parent);
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
        
        public Vector3 GuardPosition
        {
            get => Hostile.GuardPosition;
            set => Hostile.GuardPosition = value;
        }
    }
}
