using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Swiftness : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Swiftness.png");
        public override void Use()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {

        }
        public override string Description => Translations.Get("swiftness_desc");
        public override string DisplayName => Translations.Get("swiftness_skill");
    }
}