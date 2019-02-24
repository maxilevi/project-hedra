using System;
using System.Globalization;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Raven : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Raven.png");
        protected override bool EquipWeapons => false;
        protected override bool CanMoveWhileCasting => false;
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherWhistle.dae");
        private IEntity _raven;
        private bool IsActive => _raven != null && !_raven.Disposed;

        protected override void OnAnimationEnd()
        {
            Player.Movement.Orientate();
            SpawnRaven();
        }

        private void SpawnRaven()
        {
            _raven = World.SpawnMob(MobType.Bee, Player.Position + Player.Orientation * 12, Utils.Rng);
            _raven.RemoveComponent(_raven.SearchComponent<BasicAIComponent>());
            _raven.RemoveComponent(_raven.SearchComponent<HealthBarComponent>());
            _raven.AddComponent(new RavenAIComponent(_raven, Player));
            _raven.AddComponent(new SelfDestructComponent(_raven, Duration));
            _raven.AddComponent(new HealthBarComponent(_raven, Translations.Get("raven_name"), HealthBarType.Friendly));
            _raven.SearchComponent<DamageComponent>().Ignore(E => E == Player);
            _raven.AttackDamage *= 1.0f + DamageMultiplier;

            SpawnEffect(_raven.Physics.TargetPosition);
        }

        private static void SpawnEffect(Vector3 Position)
        {
            SoundPlayer.PlaySound(SoundType.DarkSound, Position);
            World.Particles.Color = Vector4.One * .25f;
            World.Particles.ParticleLifetime = 1.25f;
            World.Particles.GravityEffect = .0f;
            World.Particles.Scale = new Vector3(.75f, .75f, .75f);
            World.Particles.Position = Position;
            World.Particles.PositionErrorMargin = Vector3.One * 1.75f;
            for (var i = 0; i < 40; i++)
            {
                World.Particles.Direction = new Vector3(Utils.Rng.NextFloat(), Utils.Rng.NextFloat(), Utils.Rng.NextFloat()) * .15f;
                World.Particles.Emit();
            }
        }

        private float Duration => 16 + Level * 2;
        private float DamageMultiplier => (Level / 7f);
        protected override int MaxLevel => 15;
        protected override bool ShouldDisable => IsActive;
        public override string Description => Translations.Get("raven_desc");
        public override string DisplayName => Translations.Get("raven_skill");
        public override float ManaCost => 45;
        public override float MaxCooldown => 18 + Duration;
        public override string[] Attributes => new[]
        {
            Translations.Get("raven_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("raven_damage_change", (int) (DamageMultiplier * 100))
        };

        private sealed class RavenAIComponent : BasicAIComponent
        {
            private readonly FollowBehaviour _follow;
            private readonly AttackBehaviour _attack;
            private readonly IEntity _owner;
            private bool _disposed;

            public RavenAIComponent(IEntity Parent, IEntity Owner) : base(Parent)
            {
                _owner = Owner;
                _owner.SearchComponent<DamageComponent>().OnDamageEvent += OnDamage;
                _owner.AfterDamaging += OnDamaging;
                _attack = new AttackBehaviour(Parent);
                _follow = new FollowBehaviour(Parent)
                {
                    Target = _owner,
                    ErrorMargin = 16
                };
            }

            public override void Update()
            {
                if (_attack.Enabled)
                {
                    _attack.Update();
                }
                else
                {
                    _follow.Update();
                }

                if (_owner.IsDead) Kill();
            }

            private void OnDamage(DamageEventArgs Args)
            {
                if(Args.Damager != _owner && Args.Damager != null)
                    _attack.SetTarget(Args.Damager);
            }

            private void OnDamaging(IEntity Target, float Damage)
            {
                _attack.SetTarget(Target);
            }

            private void Kill()
            {
                if(_disposed) return;
                _disposed = true;
                Executer.ExecuteOnMainThread(Dispose);
            }
            
            public override void Dispose()
            {
                base.Dispose();
                _owner.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamage;
                _owner.AfterDamaging -= OnDamaging;
                Raven.SpawnEffect(Parent.Physics.TargetPosition);
            }

            public override AIType Type => throw new NotImplementedException();
        }
    }
}