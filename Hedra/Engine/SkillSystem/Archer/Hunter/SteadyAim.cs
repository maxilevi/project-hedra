using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SteadyAim : PassiveSkill
    {
        protected override int MaxLevel { get; }
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SteadyAim.png");
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override void Add()
        {
            throw new System.NotImplementedException();
        }
        
        public override string Description => Translations.Get("steady_aim_desc");
        public override string DisplayName => Translations.Get("steady_aim");
    }
}