using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Warrior.Berserker
{
    public class Frenzy : ActivateDurationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Frenzy.png");
        protected override void DoEnable()
        {
            throw new System.NotImplementedException();
        }

        protected override void DoDisable()
        {
            throw new System.NotImplementedException();
        }

        protected override int MaxLevel { get; }
        protected override float Duration { get; }
        public override string Description => Translations.Get("frenzy_desc");
        public override string DisplayName => Translations.Get("frenzy_skill");
    }
}