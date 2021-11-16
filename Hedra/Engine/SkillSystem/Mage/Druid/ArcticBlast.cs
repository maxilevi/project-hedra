using System.Globalization;
using System.Numerics;
using Hedra.Components.Effects;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class ArcticBlast : PlayerRadiusEffectSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ArcticBlast.png");

        protected override Vector4 HighlightColor => new Vector4(0.4f, .4f, .85f, 1f);
        protected override float Radius => 32 + 64 * (Level / (float)MaxLevel);
        private float FreezeDuration => 3 + 4 * (Level / (float)MaxLevel);
        public override float MaxCooldown => 24;
        public override float ManaCost => 65;
        protected override int MaxLevel => 15;
        public override string Description => Translations.Get("arctic_blast_desc");
        public override string DisplayName => Translations.Get("arctic_blast_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("arctic_blast_time_change", FreezeDuration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("arctic_blast_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void Apply(IEntity Entity)
        {
            Entity.AddComponent(new FreezingComponent(Entity, User, FreezeDuration, 0));
        }
    }
}