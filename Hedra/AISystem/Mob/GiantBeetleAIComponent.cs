using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class GiantBeetleAIComponent : BaseBeetleAIComponent, IGuardAIComponent
    {
        private GuardHostileBehaviour Hostile { get; set; }
        public GiantBeetleAIComponent(IEntity Entity) : base(Entity)
        {
        }
        public Vector3 GuardPosition
        {
            get => Hostile.GuardPosition;
            set => Hostile.GuardPosition = value;
        }

        protected override HostileBehaviour GetHostileBehaviour(IEntity Parent)
        {
            return Hostile = new GuardHostileBehaviour(Parent);
        }

        protected override AttackBehaviour GetAttackBehaviour(IEntity Parent)
        {
            return new GiantBeetleAttackBehaviour(Parent);
        }
    }
}