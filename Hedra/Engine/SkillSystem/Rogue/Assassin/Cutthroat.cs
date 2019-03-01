using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class Cutthroat : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Cutthroat.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("cutthroat_desc");
        public override string DisplayName => Translations.Get("cutthroat_skill");
    }
}