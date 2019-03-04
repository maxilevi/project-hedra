using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class BattleCry : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/BattleCry.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("battle_cry_skill");
        public override string DisplayName => Translations.Get("battle_cry_skill");
    }
}