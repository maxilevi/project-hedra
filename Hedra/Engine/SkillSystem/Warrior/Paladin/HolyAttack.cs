using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class HolyAttack : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HolyAttack.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorIdle.dae");
        
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("holy_attack_desc");
        public override string DisplayName => Translations.Get("holy_attack_skill");
    }
}