using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class Meditation : PassiveSkill
    {
        protected override int MaxLevel => 15;
        private const float ChangePerLevel = 0.5f;
        private float _accumulatedChange;
        
        protected override void Remove()
        {
            Player.ManaRegenFactor -= _accumulatedChange;
            _accumulatedChange = 0;
        }

        protected override void OnChange()
        {
            Player.ManaRegenFactor -= _accumulatedChange;
            _accumulatedChange = ManaRegenFormula();
            Player.ManaRegenFactor += _accumulatedChange; 
        }

        private float ManaRegenFormula()
        {
            return ChangePerLevel * Level;
        }
        
        public override string Description => $"Grants +{(ManaRegenFormula()):0.0} MP regen.";
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Meditation.png");
        public override string DisplayName => "Meditation";
    }
}