using System.Globalization;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public class GroundFissure : SpecialRangedAttackSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/GroundFissure.png");

        protected override void OnMove(Projectile Proj)
        {
            base.OnMove(Proj);
        }

        private void CreateExplosion(Vector3 Position)
        {
            
        }

        protected override int MaxLevel => 15;
        private float Radius => 1;
        private float Duration => 1;
        private float TotalDamage => 1;
        public override string Description => Translations.Get("ground_fissure_desc");
        public override string DisplayName => Translations.Get("ground_fissure_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("ground_fissure_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("ground_fissure_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("ground_fissure_damage_change", (TotalDamage / Duration).ToString("0.0", CultureInfo.InvariantCulture))
        };
        
        protected override void OnHit(Projectile Proj, IEntity Victim)
        {
            base.OnHit(Proj, Victim);
            CreateExplosion(Victim.Position);
        }
        
        protected override void OnLand(Projectile Proj)
        {
            base.OnLand(Proj);
            CreateExplosion(Proj.Position);
        }
    }
}