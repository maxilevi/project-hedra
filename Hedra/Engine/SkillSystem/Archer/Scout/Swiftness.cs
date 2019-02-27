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
    public class Swiftness : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Swiftness.png");
        private readonly Timer _timer;
        private bool _active;
        private SpeedBonusComponent _currentSpeedBonus;
        private AttackSpeedBonusComponent _currentAttackSpeedBonus;

        public Swiftness()
        {
            _timer = new Timer(1);
        }
        
        public override void Use()
        {
            Enable();
        }
        
        public override void Update()
        {
            base.Update();
            if (_timer.Tick() && _active)
            {
                Disable();
            }
        }
        
        private void Enable()
        {
            _timer.Reset();
            _timer.AlertTime = Duration;
            _active = true;
            Player.AddComponent(_currentSpeedBonus = new SpeedBonusComponent(Player, Player.Speed * SpeedChange));
            Player.AddComponent(_currentAttackSpeedBonus = new AttackSpeedBonusComponent(Player, Player.AttackSpeed * AttackSpeedChange));
            InvokeStateUpdated();
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Color.FromArgb(128, 102, 204, 255).ToVector4() * 2;
        }
        
        private void Disable()
        {
            _active = false;
            Player.RemoveComponent(_currentSpeedBonus);
            Player.RemoveComponent(_currentAttackSpeedBonus);
            _currentSpeedBonus = null;
            _currentAttackSpeedBonus = null;
            Player.Model.Outline = false;
        }

        public override float IsAffectingModifier => _active ? 1 : 0;
        private float SpeedChange => .35f + Level * 0.025f;
        private float AttackSpeedChange => .25f + Level * .065f;
        private float Duration => 8 + Level;
        protected override int MaxLevel => 15;
        public override float MaxCooldown => Duration + 54;
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