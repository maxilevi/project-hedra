using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.AISystem.Behaviours
{
    public class GorillaWarriorAttackBehaviour : AttackBehaviour
    {
        protected const int GrowlAnimationIndex = 1;
        protected const int QuakeAnimationIndex = 0;
        protected readonly Timer GrowlTimer;

        public GorillaWarriorAttackBehaviour(Entity Parent) : base(Parent)
        {
            GrowlTimer = new Timer(8f)
            {
                AutoReset = false
            };
        }

        public override void Update()
        {
            if (FollowTimer.Tick() || !Follow.Enabled)
            {
                this.Target = null;
                Follow.Target = this.Target;
            }
            var canGrowl = Target != null && GrowlTimer.Ready && (Parent.Position - Target.Position).LengthSquared() > 16 * 16 &&
                (Parent.Position - Target.Position).LengthSquared() < 48 * 48;

            var inAttackRange = Target != null && (InAttackRange(Target) || canGrowl);
            if (!Parent.Model.IsAttacking && Target != null && !inAttackRange)
            {
                Follow.Update();
            }
            inAttackRange = Target != null && (InAttackRange(Target) || canGrowl);
            if (!Parent.Model.IsAttacking && Target != null && inAttackRange)
            {
                FollowTimer.Reset();
                this.Attack(1.75f);
            }
            GrowlTimer.Tick();
        }

        protected override void Attack(float RangeModfier)
        {
            var asQuadruped = (QuadrupedModel)Parent.Model;
            if (GrowlTimer.Ready)
            {
                asQuadruped.Attack(asQuadruped.AttackAnimations[GrowlAnimationIndex], RangeModfier);
                GrowlTimer.Reset();
            }
            else
            {
                asQuadruped.Attack(asQuadruped.AttackAnimations[QuakeAnimationIndex], RangeModfier);
            }
        }
    }
}
