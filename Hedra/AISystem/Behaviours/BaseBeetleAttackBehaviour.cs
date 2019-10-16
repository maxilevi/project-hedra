using System.Drawing;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Sound;
using Hedra.WorldObjects;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public abstract class BaseBeetleAttackBehaviour : AttackBehaviour
    {
        protected readonly Timer SpitTimer;
        protected ChargeBehaviour Charge { get; }
        protected BaseBeetleAttackBehaviour(IEntity Parent) : base(Parent)
        {
            Charge = new ChargeBehaviour(Parent);
            SpitTimer = new Timer(SpitCooldown)
            {
                AutoReset = false
            };
        }

        public override void Update()
        {
            if (!Charge.IsCharging)
            {
                DoUpdate();
            }
            Charge.Update();
        }

        private void DoUpdate()
        {
            if (FollowTimer.Tick() || !Follow.Enabled || Target != null && Target.IsDead)
            {
                Target = null;
                Follow.Target = Target;
            }

            if (Target != null)
            {
                var inSpitRange = !Parent.IsNear(Target, 24) && Parent.IsNear(Target, 80);
                var canSpit = SpitTimer.Ready && inSpitRange && HasSpit;
                var inAttackRange = InAttackRange(Target);
                if (HasSpit && canSpit)
                {
                    FollowTimer.Reset();
                    Spit(5.5f);
                }
                else if(HasBite && inAttackRange)
                {
                    FollowTimer.Reset();
                    Bite(5.5f);
                }
                if (!Parent.Model.IsAttacking && (!inAttackRange && HasBite || !inSpitRange && HasSpit))
                {
                    if (!inSpitRange && HasSpit)
                    {
                        if(!Parent.IsNear(Target, 80))
                            Follow.Update();
                        else if(Parent.IsNear(Target, 24) && HasBite)
                            Follow.Update();
                    }
                    else
                    {
                        Follow.Update();    
                    }
                }
            }

            if (Parent.Model.IsAttacking && Target != null)
            {
                Parent.LookAt(Target);
            }
            SpitTimer.Tick();
        }

        public override void SetTarget(IEntity NewTarget)
        {
            base.SetTarget(NewTarget);
            Charge.SetTarget(NewTarget);
        }

        private void SpitAttack(IEntity Victim, QuadrupedModel Model, float RangeModifier, Animation SpitAnimation)
        {
            if(!Model.CanAttack()) return;
            void AttackHandler(Animation Sender)
            {
                Parent.LookAt(Victim);
                SpitAnimation.OnAnimationMid -= AttackHandler;
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
                    Hit.Damage(Parent.AttackDamage, this.Parent, out _);
                    Parent.AddBonusSpeedForSeconds(1.5f, 3);
                };
                World.AddWorldObject(spit);
                SoundPlayer.PlaySoundWithVariation(SoundType.BeetleSpitSound, Parent.Position);
            }
            Model.Attack(null, SpitAnimation, AttackHandler, RangeModifier);
        }

        private void BiteAttack(IEntity Victim, QuadrupedModel Model, float RangeModifier, Animation BiteAnimation)
        {
            if (!Model.CanAttack()) return;
            void AttackHandler(Animation Sender)
            {
                BiteAnimation.OnAnimationMid -= AttackHandler;
                if (!Parent.InAttackRange(Victim, RangeModifier))
                {
                    SoundPlayer.PlaySoundWithVariation(SoundType.SlashSound, Parent.Position, 1f, .5f);
                    return;
                }
                Victim.Damage(Parent.AttackDamage, Parent, out _);
            }
            Model.Attack(null, BiteAnimation, AttackHandler, RangeModifier);
        }

        private void Spit(float RangeModifier)
        {
            var asQuadruped = (QuadrupedModel) Parent.Model;
            SpitAttack(Target, asQuadruped, RangeModifier, GetSpitAnimation(asQuadruped));
            SpitTimer.Reset();
        }

        private void Bite(float RangeModifier)
        {
            var asQuadruped = (QuadrupedModel) Parent.Model;
            BiteAttack(Target, asQuadruped, RangeModifier, GetBiteAnimation(asQuadruped));
        }

        protected override void Attack(float RangeModifier)
        {
            if (SpitTimer.Ready && HasSpit)
                Spit(RangeModifier);
            else if (HasBite)
                Bite(RangeModifier);
        }

        protected abstract Animation GetBiteAnimation(QuadrupedModel Model);
        protected abstract Animation GetSpitAnimation(QuadrupedModel Model);

        protected virtual float SpitCooldown => 1;
        protected abstract bool HasSpit { get; }
        private bool HasBite => true;
    }
}