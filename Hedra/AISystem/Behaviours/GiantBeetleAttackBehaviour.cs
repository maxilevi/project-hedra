using System.Drawing;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class GiantBeetleAttackBehaviour : AttackBehaviour
    {
        protected const int SpitAnimationIndex = 1;
        protected const int BiteAnimationIndex = 0;
        protected readonly Timer SpitTimer;

        public GiantBeetleAttackBehaviour(IEntity Parent) : base(Parent)
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
            var canSpit = Target != null && SpitTimer.Ready &&
                          (Parent.Position - Target.Position).LengthSquared > 24 * 24 && (Parent.Position - Target.Position).LengthSquared < 80 * 80;
            var inAttackRange = Target != null && (InAttackRange(Target) || canSpit);
            if (!Parent.Model.IsAttacking && Target != null && !inAttackRange)
            {
                Follow.Update();
            }
            inAttackRange = Target != null && (InAttackRange(Target) || canSpit);
            if (Target != null && inAttackRange)
            {
                FollowTimer.Reset();
                this.Attack(5.5f);
            }
            SpitTimer.Tick();
        }

        protected override void Attack(float RangeModifier)
        {
            var asQuadruped = (QuadrupedModel) Parent.Model;
            if (SpitTimer.Ready)
            {
                this.SpitAttack(this.Target, asQuadruped, RangeModifier);
                SpitTimer.Reset();
            }
            else
            {
                this.BiteAttack(this.Target, asQuadruped, RangeModifier);
            }
        }

        private void SpitAttack(IEntity Victim, QuadrupedModel Model, float RangeModifier)
        {
            if(!Model.CanAttack()) return;
            var spitAnimation = Model.AttackAnimations[SpitAnimationIndex];
            void AttackHandler(Animation Sender)
            {
                Parent.RotateTowards(Victim);
                spitAnimation.OnAnimationMid -= AttackHandler;
                var direction = (Victim.Position - Parent.Position).NormalizedFast();
                var spit = new ParticleProjectile(Parent, Parent.Position + Parent.Orientation * 2f + Vector3.UnitY * 2f)
                {
                    Propulsion = direction * 2f,
                    Color = Color.LawnGreen.ToVector4() * .5f,
                    UseLight = false
                };
                spit.HitEventHandler += delegate(Projectile Projectile, IEntity Hit)
                {
                    Hit.KnockForSeconds(3);
                    Hit.Damage(Parent.AttackDamage, this.Parent, out float exp);
                    Parent.AddBonusSpeedForSeconds(1.5f, 3);
                };
                SoundPlayer.PlaySoundWithVariation(SoundType.BeetleSpitSound, Parent.Position);
            }
            Model.Attack(null, spitAnimation, AttackHandler, RangeModifier);
        }

        private void BiteAttack(IEntity Victim, QuadrupedModel Model, float RangeModifier)
        {
            if (!Model.CanAttack()) return;
            var biteAnimation = Model.AttackAnimations[BiteAnimationIndex];
            void AttackHandler(Animation Sender)
            {
                biteAnimation.OnAnimationMid -= AttackHandler;
                if (!Parent.InAttackRange(Victim, RangeModifier))
                {
                    SoundPlayer.PlaySoundWithVariation(SoundType.SlashSound, Parent.Position, 1f, .5f);
                    return;
                }
                Victim.Damage(Parent.AttackDamage, this.Parent, out float exp);
            }
            Model.Attack(null, biteAnimation, AttackHandler, RangeModifier);
        }
    }
}
