using System;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.EnvironmentSystem
{
    public class WeatherManager
    {
        public bool IsRaining { get; private set; }
        private readonly ParticleSystem _rain;
        private readonly Random _rng;
        private readonly Timer _rainTimer;
        private int _previousTrackIndex;
        private bool _previousTrackState;

        public WeatherManager()
        {
            _rain = new ParticleSystem(Vector3.Zero)
            {
                MaxParticles = ParticleSystem.MaxParticleCount * 6
            };
            _rng = new Random();
            _rainTimer = new Timer(300f);
        }

        public void Update(Chunk UnderChunk)
        {
            if(!UnderChunk.Biome.Sky.CanRain) return;

            if (IsRaining)
            {
                if (_rainTimer.Tick())
                {
                    IsRaining = false;
                    SoundtrackManager.PlayTrack(_previousTrackIndex, _previousTrackState);
                }
            }
            if (!IsRaining)
            {
                if (_rng.Next(0, 6000) == 1)
                {
                    IsRaining = true;
                    _previousTrackIndex = SoundtrackManager.TrackIndex;
                    _previousTrackState = SoundtrackManager.RepeatTrack;
                    SoundtrackManager.PlayTrack(SoundtrackManager.RainIndex, true);
                }
            }

            if (IsRaining)
            {
                this.ManageRain(UnderChunk);
            }
        }

        public void ManageRain(Chunk UnderChunk)
        {
            if (GameManager.InMenu || GameManager.Player == null || UnderChunk == null) return;

            _rain.Position = GameManager.Player.Position + Vector3.UnitY * 384;
            _rain.Color = new Vector4(UnderChunk.Biome.Colors.WaterColor.Xyz * .8f, .7f);
            _rain.VariateUniformly = true;
            _rain.Grayscale = false;
            _rain.PositionErrorMargin = Vector3.One * new Vector3(256f, 16, 256f);
            _rain.GravityEffect = 0.2f;
            _rain.ScaleErrorMargin = Vector3.One * .25f;
            _rain.RandomRotation = true;
            _rain.Scale = Vector3.One * 1f;
            _rain.ParticleLifetime = 6.0f;

            for (var i = 0; i < 20; i++)
            {
                _rain.Emit();
            }

            _rain.Particles[_rain.Particles.Count - 1].Collides = true;
        }
    }
}
