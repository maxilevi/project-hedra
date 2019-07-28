using System.Net.NetworkInformation;
using Hedra.Core;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Necromancer
{
    public class BloodProjectile : ParticleProjectile
    {
        private readonly IEntity _to;
        private readonly bool _initialized;
        
        public BloodProjectile(IEntity From, IEntity To, Vector3 Origin) : base(From, Origin)
        {
            _to = To;
            _initialized = true;
        }

        public override void Update()
        {
            if(!_initialized) return;
            Direction = (_to.Position + Vector3.UnitY * _to.Model.Height * .5f - Position).NormalizedFast();
            base.Update();
        }

        protected override void DoParticles()
        {
            Particles.Position = Position;
            Particles.Color = new Vector4(.5f, 0, 0, 1);
            Particles.Shape = ParticleShape.Sphere;
            Particles.ParticleLifetime = .3f;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = new Vector3(1f, 1f, 1f);
            Particles.Scale = Vector3.One * .35f;
            Particles.ScaleErrorMargin = new Vector3(.4f, .4f, .4f);
            Particles.VariateUniformly = true;
            for(var i = 0; i < 10; ++i) Particles.Emit();
        }
    }
}