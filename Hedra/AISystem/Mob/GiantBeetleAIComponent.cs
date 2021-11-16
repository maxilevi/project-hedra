using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class GiantBeetleAIComponent : BaseBeetleAIComponent, IGuardAIComponent
    {
        public GiantBeetleAIComponent(IEntity Entity) : base(Entity)
        {
        }

        private GuardHostileBehaviour Hostile { get; set; }

        public Vector3 GuardPosition
        {
            get => Hostile.GuardPosition;
            set => Hostile.GuardPosition = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            Hostile.Dispose();
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