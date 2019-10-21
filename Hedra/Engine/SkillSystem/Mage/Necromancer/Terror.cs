using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.Particles;
using Hedra.Sound;
using System.Numerics;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class Terror : RadiusEffectSkill<ISkilledAnimableEntity>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Terror.png");

        protected override Vector4 HighlightColor => Vector4.One * .2f;

        protected override void Apply(IEntity Entity)
        {
            if(Entity.SearchComponent<FearComponent>() == null)
                Entity.AddComponent(new FearComponent(Entity, User, Duration, Slowness));
        }
        
        protected override float Radius => 24 + 32 * (Level / (float) MaxLevel);
        private float Duration => 4 + 5f * (Level / (float) MaxLevel);
        private float Slowness => 85 - 15f * (Level / (float) MaxLevel);
        public override float MaxCooldown => 32 - 8 * (Level / (float) MaxLevel) + Duration;
        public override float ManaCost => 45;
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("terror_desc");
        public override string DisplayName => Translations.Get("terror_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("terror_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("terror_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("terror_slowness_change", (int) (100-Slowness))
        };
    }
}