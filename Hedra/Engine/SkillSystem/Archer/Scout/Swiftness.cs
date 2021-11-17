using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using SixLabors.ImageSharp;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Swiftness : PlayerActivateDurationSkill
    {
        private AttackSpeedBonusComponent _currentAttackSpeedBonus;
        private SpeedBonusComponent _currentSpeedBonus;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Swiftness.png");

        private float SpeedChange => .35f + Level * 0.025f;
        private float AttackSpeedChange => .25f + Level * .065f;
        protected override float Duration => 8 + Level;
        protected override int MaxLevel => 15;
        protected override float CooldownDuration => 54;
        public override float ManaCost => 60;
        public override string Description => Translations.Get("swiftness_desc");
        public override string DisplayName => Translations.Get("swiftness_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("swiftness_time_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("swiftness_speed_bonus_change", (int)(SpeedChange * 100)),
            Translations.Get("swiftness_attack_speed_bonus_change", (int)(AttackSpeedChange * 100))
        };

        protected override void DoEnable()
        {
            User.AddComponent(_currentSpeedBonus = new SpeedBonusComponent(User, User.Speed * SpeedChange));
            User.AddComponent(_currentAttackSpeedBonus =
                new AttackSpeedBonusComponent(User, User.AttackSpeed * AttackSpeedChange));
            User.Model.Outline = true;
            User.Model.OutlineColor = Color.FromRgba(102, 204, 255, 128).AsVector4() * 2;
        }

        protected override void DoDisable()
        {
            User.RemoveComponent(_currentSpeedBonus);
            User.RemoveComponent(_currentAttackSpeedBonus);
            _currentSpeedBonus = null;
            _currentAttackSpeedBonus = null;
            User.Model.Outline = false;
        }
    }
}