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
    public class Raven : CompanionSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Raven.png");
        protected override bool EquipWeapons => false;
        protected override bool CanMoveWhileCasting => false;
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
            _raven.AddComponent(new MinionAIComponent(_raven, Player));
            _raven.AddComponent(new SelfDestructComponent(_raven, Duration));
            _raven.AddComponent(new HealthBarComponent(_raven, Translations.Get("raven_name"), HealthBarType.Friendly));
            _raven.SearchComponent<DamageComponent>().Ignore(E => E == Player || E == Player.Pet.Pet);
            _raven.AttackDamage *= 1.0f + DamageMultiplier;
            _raven.SearchComponent<DamageComponent>().OnDeadEvent += A => SpawnEffect(_raven.Physics.TargetPosition); 

            SpawnEffect(_raven.Physics.TargetPosition);
        }

        private static void SpawnEffect(Vector3 Position)
        {
            SoundPlayer.PlaySound(SoundType.DarkSound, Position);
            SkillUtils.DarkSpawnParticles(Position);
        }

        private float Duration => 18 + Level * 2;
        private float DamageMultiplier => (Level / 7f);
        protected override int MaxLevel => 15;
        protected override bool ShouldDisable => IsActive;
        public override string Description => Translations.Get("raven_desc");
        public override string DisplayName => Translations.Get("raven_skill");
        public override float ManaCost => 45;
        public override float MaxCooldown => 22 + Duration;
        public override string[] Attributes => new[]
        {
            Translations.Get("raven_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("raven_damage_change", (int) (DamageMultiplier * 100))
        };
    }
}