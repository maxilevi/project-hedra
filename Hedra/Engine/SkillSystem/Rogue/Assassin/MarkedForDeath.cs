using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class MarkedForDeath : SpecialAttackSkill<RogueWeapon>
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/MarkedForDeath.png");
        
        protected override void BeforeUse(RogueWeapon Weapon)
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("marked_for_death_desc");
        public override string DisplayName => Translations.Get("marked_for_death_skill");
    }
}