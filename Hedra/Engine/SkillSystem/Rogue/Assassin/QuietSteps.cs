using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class QuietSteps : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/QuietSteps.png");
        private float _previousValue;
        
        protected override void Add()
        {
            User.Attributes.MobAggroModifier -= _previousValue = AggroChange;
        }

        protected override void Remove()
        {
            User.Attributes.MobAggroModifier += _previousValue;
            _previousValue = 0;
        }

        private float AggroChange => .15f + .35f * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("quiet_steps_desc");
        public override string DisplayName => Translations.Get("quiet_steps_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("quiet_steps_aggro_change", (int) (AggroChange * 100))
        };
    }
}