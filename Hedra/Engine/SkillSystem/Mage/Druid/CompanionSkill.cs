using System.Globalization;
using System.Linq;
using Hedra.AISystem;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using System.Numerics;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public abstract class CompanionSkill : SingleAnimationSkill<IPlayer>
    {
        private static bool IsActive => _minion != null;
        private static IEntity _minion;
        protected sealed override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherWhistle.dae");
        protected override bool EquipWeapons => false;
        protected override bool CanMoveWhileCasting => false;
        protected override int MaxLevel => 20;
        protected override bool ShouldDisable => IsActive && !_isActiveInstance;
        private bool _isActiveInstance;

        protected override void DoUse()
        {
            if (_isActiveInstance)
            {
                Reset();
            }
            else
            {
                base.DoUse();
            }
        }

        public override bool MeetsRequirements()
        {
            return _isActiveInstance || base.MeetsRequirements();
        }

        protected override void OnAnimationEnd()
        {
            User.Movement.Orientate();
            _minion = SpawnMinion();
            _isActiveInstance = true;
            Tint = new Vector3(2, .2f, .2f);
        }

        protected virtual IEntity SpawnMinion()
        {
            var minion = World.SpawnMob(CompanionType, User.Position + User.Orientation * 12, Utils.Rng);
            minion.RemoveComponent(minion.SearchComponent<BasicAIComponent>());
            minion.RemoveComponent(minion.SearchComponent<HealthBarComponent>());
            minion.AddComponent(new MinionAIComponent(minion, User));
            minion.AddComponent(new HealthBarComponent(minion, Translations.Get($"{Keyword}_companion_name"), HealthBarType.Friendly));
            minion.SearchComponent<DamageComponent>().Ignore(E => E == User || E == User.Companion.Entity);
            minion.SearchComponent<DamageComponent>().OnDeadEvent += A => SpawnEffect(minion.Position);
            var masterySkill = (CompanionMastery) User.Toolbar.Skills.First(S => S.GetType() == typeof(CompanionMastery));
            minion.MaxHealth *= masterySkill.HealthMultiplier;
            minion.AttackResistance *= masterySkill.ResistanceMultiplier;
            minion.AttackDamage *= masterySkill.DamageMultiplier;
            minion.Health = minion.MaxHealth;
            SpawnEffect(minion.Position);
            return minion;
        }

        public override void Update()
        {
            base.Update();
            if (IsActive && _isActiveInstance)
            {
                if (_minion.Disposed) Disable();
            }
            if (_isActiveInstance)
            {
                Tint = new Vector3(1, .2f, .2f);
                Cooldown = 0;
            }
            else
            {
                Tint = Vector3.One;
            }
        }

        private void Disable()
        {
            _minion = null;
            _isActiveInstance = false;
            Tint = Vector3.One;
            Cooldown = MaxCooldown;
        }

        private void Reset()
        {
            _minion.Dispose();
            Disable();
        }
        
        protected abstract MobType CompanionType { get; }
        
        protected abstract void SpawnEffect(Vector3 TargetPosition);

        protected override bool HasCooldown => !IsActive;
        public override float ManaCost => _isActiveInstance ? 0 : 140 - 70 * (Level / (float) MaxLevel);
        public override float MaxCooldown => 54 - 30 * (Level / (float) MaxLevel);
        public sealed override string Description => Translations.Get($"{Keyword}_companion_desc");
        public sealed override string DisplayName => Translations.Get($"{Keyword}_companion_skill");

        public override string[] Attributes
        {
            get
            {
                var template = World.MobFactory.GetFactory(CompanionType.ToString().ToLowerInvariant());
                var masterySkill = (CompanionMastery) User.Toolbar.Skills.First(S => S.GetType() == typeof(CompanionMastery));
                return new[]
                {
                    Translations.Get("companion_mastery_health_change", (template.MaxHealth * masterySkill.HealthMultiplier).ToString("0.0", CultureInfo.InvariantCulture)),
                    Translations.Get("companion_mastery_damage_change", (template.AttackDamage * masterySkill.DamageMultiplier).ToString("0.0", CultureInfo.InvariantCulture))
                };
            }
        }
        protected abstract string Keyword { get; }
    }
}