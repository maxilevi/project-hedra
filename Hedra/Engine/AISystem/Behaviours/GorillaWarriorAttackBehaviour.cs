using System;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.AISystem.Behaviours
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
            var inAttackRange = Target != null && (Parent.InAttackRange(Target) || GrowlTimer.Ready && (Parent.Position - Target.Position).LengthSquared > 16 * 16);
            if (!Parent.Model.IsAttacking && Target != null && !inAttackRange)
            {
                Follow.Update();
            }
            if (inAttackRange)
            {
                FollowTimer.Reset();
                this.Attack(1f);
            }
            GrowlTimer.Tick();
        }

        public override void Attack(float RangeModfier)
        {
            var asQuadruped = (QuadrupedModel)Parent.Model;
            if (GrowlTimer.Ready)
            {
                asQuadruped.Attack(asQuadruped.AttackAnimations[GrowlAnimationIndex]);
                GrowlTimer.Reset();
            }
            else
            {
                asQuadruped.Attack(asQuadruped.AttackAnimations[QuakeAnimationIndex]);
            }
        }
    }
}
