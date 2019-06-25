using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Localization;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Meditation : PassiveSkill
    {
        protected override int MaxLevel => 15;
        private const float ChangePerLevel = 0.5f;
        private float _accumulatedChange;
        
        protected override void Add()
        {
            User.ManaRegenFactor += _accumulatedChange = ManaRegenFormula(); 
        }

        protected override void Remove()
        {
            User.ManaRegenFactor -= _accumulatedChange;
            _accumulatedChange = 0;
        }

        
        private float ManaRegenFormula()
        {
            return ChangePerLevel * Level;
        }
        
        public override string Description => Translations.Get("meditation_desc", ManaRegenFormula().ToString("0.0", CultureInfo.InvariantCulture));
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Meditation.png");
        public override string DisplayName => Translations.Get("meditation_skill");
    }
}