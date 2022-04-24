using System;
using System.Numerics;
using Hedra.Engine.Rendering.Particles;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class StructureAmbientHandler : IDisposable
    {
        private readonly IStructureWithRadius _parent;
        private readonly ParticleSystem _particles;
        private readonly int _passedTime = 0;
        private float _cementeryTime;
        private bool _inCementery;
        private float _oldCementeryTime;
        private float _oldTime;
        private bool _restoreSoundPlayed;
        private bool _shouldUpdateTime;
        private float _targetCementeryTime;
        private readonly TimeHandler _timeHandler;

        public StructureAmbientHandler(IStructureWithRadius Parent, int Time)
        {
            _timeHandler = new TimeHandler(Time, SoundType.DarkSound);
            _parent = Parent;
            _particles = new ParticleSystem();
        }

        public void Dispose()
        {
            _particles.Dispose();
            _timeHandler.Dispose();
        }

        public void Update()
        {
            HandleSky();
            HandleParticles();
        }

        private void HandleSky()
        {
            var wasInCementery = _inCementery;

            _inCementery = (_parent.Position.Xz() - GameManager.Player.Position.Xz()).LengthSquared() <
                _parent.Radius * _parent.Radius * .5f * .5f && !_parent.Completed;

            if (_inCementery && !wasInCementery)
                _timeHandler.Apply();
            else if (!_inCementery && wasInCementery) _timeHandler.Remove();

            _timeHandler.Update();
        }

        private void HandleParticles()
        {
            if (_parent.Completed && !_restoreSoundPlayed)
            {
                _restoreSoundPlayed = true;
                SoundPlayer.PlaySound(SoundType.DarkSound, GameManager.Player.Position);
            }

            if (!_parent.Completed && (_parent.Position - GameManager.Player.Position).Xz().LengthSquared()
                < _parent.Radius * _parent.Radius)
                if (_passedTime % 2 == 0)
                {
                    _particles.Color = Particle3D.AshColor;
                    _particles.VariateUniformly = false;
                    _particles.Position = GameManager.Player.Position + Vector3.UnitY * 1f;
                    _particles.Scale = Vector3.One * .85f;
                    _particles.ScaleErrorMargin = new Vector3(.05f, .05f, .05f);
                    _particles.Direction = Vector3.UnitY * 0f;
                    _particles.ParticleLifetime = 2f;
                    _particles.GravityEffect = -0.000f;
                    _particles.PositionErrorMargin = new Vector3(64f, 12f, 64f);
                    _particles.Grayscale = true;

                    _particles.Emit();
                }
        }
    }
}