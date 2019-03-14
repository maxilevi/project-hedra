using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class BearCompanion : CompanionSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BearCompanion.png");
        protected override string Keyword => "bear";
    }
}