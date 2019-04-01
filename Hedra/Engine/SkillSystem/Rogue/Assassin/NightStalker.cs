using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class NightStalker : ConditionedPassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/NightStalker.png");
        private float _previousValue;
        
        protected override void DoAdd()
        {
            Player.Attributes.CooldownReductionModifier += _previousValue = CooldownReduction;
        }
        
        protected override void DoRemove()
        {
            Player.Attributes.CooldownReductionModifier -= _previousValue;
            _previousValue = 0;
        }

        protected override bool CheckIfCanDo()
        {
            return SkyManager.IsNight;
        }

        private float CooldownReduction => .15f + .30f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("night_stalker_desc");
        public override string DisplayName => Translations.Get("night_stalker_skill");
        protected override int MaxLevel => 15;
        public override string[] Attributes => new[]
        {
            Translations.Get("night_stalker_cooldown_change", (int)(CooldownReduction * 100f))
        };
    }
}