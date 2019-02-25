using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class HotPursuit : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HotPursuit.png");
        protected override int MaxLevel => 15;
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        private float SpeedChange => .1f;
        public override string Description => Translations.Get("hot_pursuit_desc");
        public override string DisplayName => Translations.Get("hot_pursuit_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("hot_pursuit_speed_change", (int) (SpeedChange * 100))
        };
    }
}