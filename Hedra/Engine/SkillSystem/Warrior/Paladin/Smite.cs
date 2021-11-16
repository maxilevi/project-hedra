using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Smite : PassiveSkill
    {
        private float _previousValue;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Smite.png");

        protected override int MaxLevel => 15;
        private float UndeadMultiplier => 2.5f * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("smite_desc");
        public override string DisplayName => Translations.Get("smite_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("smite_damage_multiplier", (int)(UndeadMultiplier * 100))
        };

        protected override void Add()
        {
            User.Attributes.UndeadDamageModifier += _previousValue = UndeadMultiplier;
        }

        protected override void Remove()
        {
            User.Attributes.UndeadDamageModifier -= _previousValue;
            _previousValue = 0;
        }
    }
}