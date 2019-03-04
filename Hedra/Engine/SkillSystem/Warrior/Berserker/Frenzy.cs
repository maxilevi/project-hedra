using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Frenzy : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Frenzy.png");
        private AttackSpeedBonusComponent _component;
        
        protected override void DoEnable()
        {
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Colors.Red;
            Player.AddComponent(_component = new AttackSpeedBonusComponent(Player, Player.BaseAttackSpeed * AttackSpeedChange));
            Player.Damage(HealthCost, null, out _);
        }

        protected override void DoDisable()
        {
            Player.Model.Outline = false;
            Player.RemoveComponent(_component);
            _component = null;
        }

        protected override float CooldownDuration => 22;
        protected override bool ShouldDisable => Player.Health < HealthCost;
        protected override int MaxLevel => 15;
        protected override float Duration => 5 + 5 * (Level / (float) MaxLevel);
        public override float ManaCost => 0;
        private float AttackSpeedChange => .15f + 1.35f * (Level / (float) MaxLevel);
        private float HealthCost => 20 + 60 * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("frenzy_desc");
        public override string DisplayName => Translations.Get("frenzy_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("frenzy_health_change", HealthCost.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("frenzy_attack_speed_change", (int)(AttackSpeedChange * 100)),
            Translations.Get("frenzy_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}