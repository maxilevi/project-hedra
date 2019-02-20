using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Scavenge : PassiveSkill
    {
        protected override int MaxLevel => 15;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Scavenge.png");
        private float _previousValue;
        
        protected override void Remove()
        {
            Player.Attributes.FoodDropChanceModifier -= _previousValue;
        }

        protected override void Add()
        {
            Player.Attributes.FoodDropChanceModifier += _previousValue = Formula(Level);
        }

        private float Formula(int Factor)
        {
            return Factor * .1f;
        }
        
        public override string Description => Translations.Get("scavenge_desc", (int)(Formula(Level + 1) * 100));
        public override string DisplayName => Translations.Get("scavenge_skill");
    }
}