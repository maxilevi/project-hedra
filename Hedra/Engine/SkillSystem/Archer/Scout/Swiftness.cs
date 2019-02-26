using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Swiftness : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Swiftness.png");
        public override void Use()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {

        }

        private float SpeedChange => 1;
        private float AttackSpeedChange => 1;
        private float Duration => 1;
        public override string Description => Translations.Get("swiftness_desc");
        public override string DisplayName => Translations.Get("swiftness_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("swiftness_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("swiftness_speed_bonus_change", SpeedChange.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("swiftness_attack_speed_bonus_change", AttackSpeedChange.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}