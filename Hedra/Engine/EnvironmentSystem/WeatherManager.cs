using System;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Particles;
using Hedra.Game;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.EnvironmentSystem
{
    public class WeatherManager
    {
        private readonly ParticleSystem _rain;
        private readonly Random _rng;
        private readonly Timer _rainTimer;
        private int _previousTrackIndex;
        private bool _previousTrackState;
        private bool _isRaining;

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
            if (IsRaining)
            {
                if (_rainTimer.Tick() || !UnderChunk.Biome.Sky.CanRain)
                {
                    IsRaining = false;
                }
            }
            if (!IsRaining && UnderChunk.Biome.Sky.CanRain)
            {
                if (_rng.Next(0, 6000) == 1)
                {
                    IsRaining = true;
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
            _rain.GravityEffect = 0.0f;
            _rain.ScaleErrorMargin = Vector3.One * .25f;
            _rain.RandomRotation = true;
            _rain.Direction = Vector3.UnitY * -4.5f;
            _rain.Scale = Vector3.One * 1f;
            _rain.ParticleLifetime = 6.0f;

            for (var i = 0; i < 10; i++)
            {
                _rain.Emit();
            }
        }

        public bool IsRaining
        {
            get => _isRaining;
            set
            {
                throw new NotImplementedException();
                if (_isRaining == value) return;
                _isRaining = value;
                /*if (_isRaining)
                {
                    _previousTrackIndex = SoundtrackManager.TrackIndex;
                    _previousTrackState = SoundtrackManager.RepeatTrack;
                    SoundtrackManager.PlayTrack(SoundtrackManager.RainIndex, true);
                }
                else
                {
                    SoundtrackManager.PlayTrack(_previousTrackIndex, _previousTrackState);
                }*/
            }
        }
    }
}
