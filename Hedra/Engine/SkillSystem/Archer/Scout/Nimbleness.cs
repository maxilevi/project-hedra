using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class Nimbleness : PassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Nimbleness.png");
        protected override int MaxLevel => 15;
        private float _previousValue;
        
        protected override void Remove()
        {
            User.Attributes.TumbleDistanceModifier -= _previousValue;
            _previousValue = 0;
        }

        protected override void Add()
        {
            User.Attributes.TumbleDistanceModifier += _previousValue = DistanceChange;
        }
        
        /* This will give us a range of 106% -> 200% */
        private float DistanceChange => Level / 10f;
        public override string Description => Translations.Get("nimbleness_desc");
        public override string DisplayName => Translations.Get("nimbleness_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("nimbleness_distance_change", (int) (DistanceChange * 100))
        };
    }
}