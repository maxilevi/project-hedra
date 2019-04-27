using System.Globalization;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class SiphonBlood : BloodSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SiphonBlood.png");
        protected override float AnimationSpeed => 1.5f;

        protected override void SpawnParticle(IEntity Victim)
        {
            LaunchParticle(User, Victim, User, OnReached);
        }

        protected override void OnStart(IEntity Victim)
        {
            Victim.Model.Outline = true;
            Victim.Model.OutlineColor = Colors.Red;
            Victim.Damage(Damage, User, out var xp);
            if(User is IHumanoid humanoid)
                humanoid.XP += xp;
        }
        
        private void OnReached(IEntity From, IEntity To)
        {
            To.Health += HealthBonus;
            To.Model.Outline = true;
            To.Model.OutlineColor = Colors.FullHealthGreen;
            To.Model.Tint = Colors.FullHealthGreen;
            TaskScheduler.After(1f, () =>
            {
                To.Model.Outline = false;
                To.Model.Tint = Colors.White;
            });
            From.Model.Outline = false;
        }
        
        private float Damage => 22 + 22 * (Level / (float)MaxLevel);
        private float HealthBonus => Damage * .75f;
        public override float ManaCost => 45;
        public override float MaxCooldown => 18 - 6 * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("siphon_blood_desc");
        public override string DisplayName => Translations.Get("siphon_blood_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("siphon_blood_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("siphon_blood_health_change", HealthBonus.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}