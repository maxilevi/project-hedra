using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class FireEnchant : WeaponBonusSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/FireEnchant.png");

        protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage)
        {
            if (Victim.SearchComponent<BurningComponent>() == null)
            {
                Victim.AddComponent(new BurningComponent(Victim, User, 5f, Damage));
            }
        }

        public override void Update()
        {
            base.Update();
            if (IsActive)
            {
                World.Particles.Color = Particle3D.FireColor;
                World.Particles.VariateUniformly = false;
                World.Particles.Position = User.LeftWeapon.MainMesh.TransformPoint(Vector3.UnitY * (User.LeftWeapon.MainMesh.CullingBox?.Size.Y * .25f ?? 0));
                World.Particles.Scale = Vector3.One * .2f;
                World.Particles.ScaleErrorMargin = new Vector3(.1f,.1f,.1f);
                World.Particles.Direction = Vector3.UnitY * .2f;
                World.Particles.ParticleLifetime = 0.75f;
                World.Particles.GravityEffect = 0.0f;
                World.Particles.PositionErrorMargin = Vector3.One * (User.LeftWeapon.MainMesh.CullingBox?.Size.Y * .25f ?? 0);
                World.Particles.Emit();
            }
        }

        protected override Vector4 OutlineColor => Colors.OrangeRed;
        protected override int MaxLevel => 15;
        public override float ManaCost => 45;
        public override float MaxCooldown => 17;
        private float Damage => 15 + 30 * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("fire_enchant_desc");
        public override string DisplayName => Translations.Get("fire_enchant_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("fire_enchant_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}