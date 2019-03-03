using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Berserk : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Berserk.png");
        protected override void DoEnable()
        {
            throw new System.NotImplementedException();
        }

        protected override void DoDisable()
        {
            throw new System.NotImplementedException();
        }

        protected override float Duration { get; }
        protected override int MaxLevel { get; }
        public override string Description => Translations.Get("berserk_desc");
        public override string DisplayName => Translations.Get("berserk_skill");
    }
}