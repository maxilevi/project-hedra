using Hedra.Components.Effects;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.Player
{
    public sealed class Fireball : ParticleProjectile
    {
        private Fireball(IEntity Parent, Vector3 Origin) : base(Parent, Origin)
        {
        }

        protected override void DoParticles()
        {
            Particles.Position = this.Position;
            Particles.Color = new Vector4(1, .3f, 0, 1);
            Particles.Shape = ParticleShape.Sphere;
            Particles.ParticleLifetime = .25f;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = new Vector3(2f, 2f, 2f);
            Particles.Scale = Vector3.One * .25f;
            Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
            Particles.VariateUniformly = false;
            for (var i = 0; i < 25; i++) Particles.Emit();
        }

        public static void Create(IHumanoid Owner, Vector3 Position, Vector3 Direction, float Damage, params IEntity[] IgnoreEntities)
        {
            var fireball = new Fireball(Owner, Position)
            {
                Color = Particle3D.FireColor,
                Direction = Direction,
                UsePhysics = false,
                UseLight = true,
                Speed = 128,
                IgnoreEntities = IgnoreEntities
            };
            fireball.HitEventHandler += delegate(Projectile Projectile, IEntity Hit)
            {
                Hit.Damage(Damage, Owner, out var exp);
                if (Utils.Rng.Next(0, 5) == 1)
                {
                    Hit.AddComponent(new BurningComponent(Hit, Owner, 5f, Damage));
                }
                Owner.XP += exp;
            };
        }
    }
}