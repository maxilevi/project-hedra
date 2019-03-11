using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class BloodProjectile : ParticleProjectile
    {     
        public BloodProjectile(IEntity Parent, Vector3 Origin) : base(Parent, Origin)
        {
        }
            
        protected override void DoParticles()
        {
            Particles.Position = Position;
            Particles.Color = new Vector4(.5f, 0, 0, 1);
            Particles.Shape = ParticleShape.Sphere;
            Particles.ParticleLifetime = .3f;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = new Vector3(2f, 2f, 2f);
            Particles.Scale = Vector3.One * .75f;
            Particles.ScaleErrorMargin = new Vector3(.4f, .4f, .4f);
            Particles.VariateUniformly = true;
            for(var i = 0; i < 5; ++i) Particles.Emit();
        }
    }
}