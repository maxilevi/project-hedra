using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public abstract class CompanionSkill : SingleAnimationSkill
    {
        private static bool _isActive;
        protected sealed override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherWhistle.dae");

        protected override int MaxLevel => 20;
        protected override bool ShouldDisable => _isActive;

        protected virtual void SpawnMinion()
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
        
        

        public override float ManaCost => 140 - 70 * (Level / (float) MaxLevel);
        public override float MaxCooldown => 54 - 30 * (Level / (float) MaxLevel);
        public sealed override string Description => Translations.Get($"{Keyword}_companion_desc");
        public sealed override string DisplayName => Translations.Get($"{Keyword}_companion_skill");
        protected abstract string Keyword { get; }
    }
}