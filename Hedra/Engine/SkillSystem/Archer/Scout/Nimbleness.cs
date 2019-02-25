using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Nimbleness : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Nimbleness.png");
        protected override int MaxLevel => 15;
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override void Add()
        {
            throw new System.NotImplementedException();
        }

        private float DistanceChange => .1f;
        public override string Description => Translations.Get("nimbleness_desc");
        public override string DisplayName => Translations.Get("nimbleness_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("nimbleness_distance_change", (int) (DistanceChange * 100))
        };
    }
}