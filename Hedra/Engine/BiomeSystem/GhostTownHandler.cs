using Hedra.Engine.Game;
using Hedra.Engine.Rendering.Particles;
using Hedra.Game;
using System.Numerics;

namespace Hedra.Engine.BiomeSystem
{
    public class GhostTownHandler : WorldHandler
    {
        private int _passedTime;
        private ParticleSystem _particles;

        public GhostTownHandler()
        {
            _particles = new ParticleSystem();
        }
        
        public override void Update()
        {
            if(_passedTime % 2 == 0)
            {
                _particles.Color = Particle3D.AshColor;
                _particles.VariateUniformly = false;
                _particles.Position = GameManager.Player.Position + Vector3.UnitY * 1f;
                _particles.Scale = Vector3.One * .85f;
                _particles.ScaleErrorMargin = new Vector3(.05f,.05f,.05f);
                _particles.Direction = Vector3.UnitY * 0f;
                _particles.ParticleLifetime = 2f;
                _particles.GravityEffect = -0.000f;
                _particles.PositionErrorMargin = new Vector3(64f, 12f, 64f);
                _particles.Grayscale = true;
                    
                _particles.Emit();
            }
        }

        public override void Dispose()
        {
            _particles.Dispose();
        }
    }
}