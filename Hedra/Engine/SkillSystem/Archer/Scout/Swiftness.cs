using System.Drawing;
using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Swiftness : ActivateDurationSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Swiftness.png");
        private SpeedBonusComponent _currentSpeedBonus;
        private AttackSpeedBonusComponent _currentAttackSpeedBonus;

        protected override void DoEnable()
        {
            Player.AddComponent(_currentSpeedBonus = new SpeedBonusComponent(Player, Player.Speed * SpeedChange));
            Player.AddComponent(_currentAttackSpeedBonus = new AttackSpeedBonusComponent(Player, Player.AttackSpeed * AttackSpeedChange));
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Color.FromArgb(128, 102, 204, 255).ToVector4() * 2;
        }
        
        protected override void DoDisable()
        {
            Player.RemoveComponent(_currentSpeedBonus);
            Player.RemoveComponent(_currentAttackSpeedBonus);
            _currentSpeedBonus = null;
            _currentAttackSpeedBonus = null;
            Player.Model.Outline = false;
        }

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
            Translations.Get("swiftness_speed_bonus_change", (int) (SpeedChange * 100)),
            Translations.Get("swiftness_attack_speed_bonus_change", (int) (AttackSpeedChange * 100))
        };
    }
}