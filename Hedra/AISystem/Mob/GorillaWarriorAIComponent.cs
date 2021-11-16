using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class GorillaWarriorAIComponent : BasicAIComponent, IGuardAIComponent
    {
        public GorillaWarriorAIComponent(Entity Entity) : base(Entity)
        {
            Retaliate = new RetaliateBehaviour(Parent);
            Hostile = new GuardHostileBehaviour(Parent);
            AlterBehaviour<AttackBehaviour>(new GorillaWarriorAttackBehaviour(Entity));
        }

        protected RetaliateBehaviour Retaliate { get; }
        protected GuardHostileBehaviour Hostile { get; }

        public override AIType Type => AIType.Hostile;

        public override void Update()
        {
            if (Retaliate.Enabled)
                Retaliate.Update();
            else
                Hostile.Update();
        }

        public Vector3 GuardPosition
        {
            get => Hostile.GuardPosition;
            set => Hostile.GuardPosition = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            Hostile.Dispose();
            Retaliate.Dispose();
        }
    }
}