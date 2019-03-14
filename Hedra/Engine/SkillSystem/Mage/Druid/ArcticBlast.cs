using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class ArcticBlast : RadiusEffectSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skill/ArcticBlast.png");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("arctic_blast_desc");
        public override string DisplayName => Translations.Get("arctic_blast_skill");
    }
}