using System;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class GiantBeetleAttackBehaviour : AttackBehaviour
    {
        protected const int SpitAnimationIndex = 1;
        protected const int BiteAnimationIndex = 0;
        protected readonly Timer SpitTimer;

        public GiantBeetleAttackBehaviour(Entity Parent) : base(Parent)
        {
            SpitTimer = new Timer(5f)
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
            var inAttackRange = Target != null && (Parent.InAttackRange(Target) || SpitTimer.Ready && (Parent.Position - Target.Position).LengthSquared > 24*24);
            if (!Parent.Model.IsAttacking && Target != null && !inAttackRange)
            {
                Follow.Update();
            }
            if (inAttackRange)
            {
                FollowTimer.Reset();
                this.Attack();
            }
            SpitTimer.Tick();
        }

        public override void Attack()
        {
            var asQuadruped = (QuadrupedModel) Parent.Model;
            if (SpitTimer.Ready)
            {
                this.SpitAttack(this.Target, asQuadruped);
                SpitTimer.Reset();
            }
            else
            {
                this.BiteAttack(this.Target, asQuadruped);
            }
        }

        private void SpitAttack(Entity Victim, QuadrupedModel Model)
        {
            var spitAnimation = Model.AttackAnimations[SpitAnimationIndex];
            void AttackHandler(Animation Sender)
            {
                Physics.LookAt(Parent, Victim);
                spitAnimation.OnAnimationMid -= AttackHandler;
                var direction = (Victim.Position - Parent.Position).NormalizedFast();
                var spit = new ParticleProjectile(Parent, Parent.Position + Parent.Orientation * 2f + direction + Vector3.UnitY * 1.25f)
                {
                    Propulsion = direction * 2f,
                    Color = Color.LawnGreen.ToVector4() * .5f,
                    UseLight = false
                };
                spit.HitEventHandler += delegate(Projectile Projectile, Entity Hit)
                {
                    Hit.KnockForSeconds(3);
                    Hit.Damage(Parent.AttackDamage, this.Parent, out float exp);
                    Parent.AddBonusSpeedForSeconds(1.5f, 3);
                };
                SoundManager.PlaySoundWithVariation(SoundType.SpitSound, Parent.Position);
            }
            spitAnimation.OnAnimationMid += AttackHandler;
            Model.Attack(null, spitAnimation, AttackHandler);
        }

        private void BiteAttack(Entity Victim, QuadrupedModel Model)
        {
            var biteAnimation = Model.AttackAnimations[BiteAnimationIndex];
            void AttackHandler(Animation Sender)
            {
                biteAnimation.OnAnimationMid -= AttackHandler;
                if (!Parent.InAttackRange(Victim))
                {
                    SoundManager.PlaySoundWithVariation(SoundType.SlashSound, Parent.Position, 1f, .5f);
                    return;
                }
                Victim.Damage(Parent.AttackDamage, this.Parent, out float exp);
            }
            Model.Attack(null, biteAnimation, AttackHandler);
        }
    }
}
