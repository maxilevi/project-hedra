using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Scavenge : PassiveSkill
    {
        private float _previousValue;
        protected override int MaxLevel => 15;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Scavenge.png");

        private float Chance => Level * .1f;
        public override string Description => Translations.Get("scavenge_desc", (int)(Chance * 100));
        public override string DisplayName => Translations.Get("scavenge_skill");

        protected override void Remove()
        {
            User.Attributes.FoodDropChanceModifier -= _previousValue;
            _previousValue = 0;
        }

        protected override void Add()
        {
            User.Attributes.FoodDropChanceModifier += _previousValue = Chance;
        }
    }
}