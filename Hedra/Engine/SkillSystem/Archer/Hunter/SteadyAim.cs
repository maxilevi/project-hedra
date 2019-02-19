using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SteadyAim : PassiveSkill
    {
        public override string Description { get; }
        public override string DisplayName { get; }
        protected override int MaxLevel { get; }
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SteadyAim.png");
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnChange()
        {
            throw new System.NotImplementedException();
        }
    }
}