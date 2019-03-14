using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class WolfCompanion : CompanionSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/WolfCompanion.png");
        protected override string Keyword => "wolf";
    }
}