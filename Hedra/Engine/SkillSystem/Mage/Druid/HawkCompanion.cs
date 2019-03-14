using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class HawkCompanion : CompanionSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HawkCompanion.png");
        protected override string Keyword => "hawk";
    }
}