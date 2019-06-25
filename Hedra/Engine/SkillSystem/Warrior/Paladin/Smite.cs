using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Localization;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Smite : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Smite.png");
        private float _previousValue;
        
        protected override void Add()
        {
            User.Attributes.UndeadDamageModifier += _previousValue = UndeadMultiplier;
        }

        protected override void Remove()
        {
            User.Attributes.UndeadDamageModifier -= _previousValue;
            _previousValue = 0;
        }

        protected override int MaxLevel => 15;
        private float UndeadMultiplier => 2.5f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("smite_desc");
        public override string DisplayName => Translations.Get("smite_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("smite_damage_multiplier", (int)(UndeadMultiplier * 100))
        };
    }
}