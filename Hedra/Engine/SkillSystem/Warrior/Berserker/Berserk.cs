using System;
using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Berserk : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Berserk.png");
        private bool _hasSideEffect;
        
        protected override void DoEnable()
        {
            Player.SearchComponent<DamageComponent>().Immune = true;
            Player.Model.Outline = true;
            Player.Model.OutlineColor = Colors.Red;
        }

        protected override void DoDisable()
        {
            Player.Model.Outline = false;
            Player.SearchComponent<DamageComponent>().Immune = false;
            Player.ComponentManager.AddComponentForSeconds(new AttackResistanceBonusComponent(Player, -Player.AttackResistance * ResistanceReduction), Duration);
            _hasSideEffect = true;
            TaskScheduler.After(Duration, () => _hasSideEffect = false);
        }

        protected override float Duration => 4 + 3f * (Level / (float)MaxLevel);
        public override float IsAffectingModifier => (float)Math.Min(1, base.IsAffectingModifier + (_hasSideEffect ? 1f : 0f));
        protected override float CooldownDuration => 28 - 6 * (Level / (float) MaxLevel);
        private float ResistanceReduction => .7f;
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("berserk_desc");
        public override string DisplayName => Translations.Get("berserk_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("berserk_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("berserk_reduction_change", (int) (ResistanceReduction * 100))
        };
    }
}