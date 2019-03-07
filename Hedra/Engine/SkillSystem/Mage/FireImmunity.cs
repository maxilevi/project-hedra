using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage
{
    public class FireImmunity : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FireImmunity.png");
        
        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }
        
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("fire_immunity_desc");
        public override string DisplayName => Translations.Get("fire_immunity_skill");
    }
}