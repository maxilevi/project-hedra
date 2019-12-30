using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.SkillSystem.Mage.Druid;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using System.Numerics;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Raven : CompanionSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Raven.png");

        protected override IEntity SpawnMinion()
        {
            var raven = base.SpawnMinion();
            raven.AttackDamage *= 1.0f + DamageMultiplier;
            raven.AddComponent(new SelfDestructComponent(raven, Duration));
            return raven;
        }

        protected override void SpawnEffect(Vector3 TargetPosition)
        {
            SkillUtils.DarkSpawnParticles(TargetPosition);
            SoundPlayer.PlaySound(SoundType.DarkSound, TargetPosition);
        }

        protected override string Keyword => "raven";        
        private float Duration => 18 + Level * 2;
        private float DamageMultiplier => (Level / 7f);
        protected override int MaxLevel => 15;
        public override float ManaCost => 45;
        public override float MaxCooldown => 22 + Duration;
        public override string[] Attributes => new[]
        {
            Translations.Get("raven_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("raven_damage_change", (int) (DamageMultiplier * 100))
        };

        protected override MobType CompanionType => MobType.Crow;
    }
}