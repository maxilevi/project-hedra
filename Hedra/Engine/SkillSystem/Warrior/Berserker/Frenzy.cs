using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.Player;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Frenzy : ActivateDurationSkill<IPlayer>
    {
        private AttackSpeedBonusComponent _component;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Frenzy.png");

        protected override float CooldownDuration => 22;
        protected override bool ShouldDisable => User.Health < HealthCost;
        protected override int MaxLevel => 15;
        protected override float Duration => 5 + 5 * (Level / (float)MaxLevel);
        public override float ManaCost => 0;
        private float AttackSpeedChange => .15f + 1.35f * (Level / (float)MaxLevel);
        private float HealthCost => 20 + 60 * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("frenzy_desc");
        public override string DisplayName => Translations.Get("frenzy_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("frenzy_health_change", HealthCost.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("frenzy_attack_speed_change", (int)(AttackSpeedChange * 100)),
            Translations.Get("frenzy_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void DoEnable()
        {
            User.Model.Outline = true;
            User.Model.OutlineColor = Colors.Red;
            User.AddComponent(
                _component = new AttackSpeedBonusComponent(User, User.BaseAttackSpeed * AttackSpeedChange));
            User.Damage(HealthCost, null, out _);
        }

        protected override void DoDisable()
        {
            User.Model.Outline = false;
            User.RemoveComponent(_component);
            _component = null;
        }
    }
}