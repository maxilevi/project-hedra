using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.Engine.Sound
{
    public class AreaSound : IDisposable
    {
        private readonly object _soundLock = new object();
        private SoundType _bufferType;
        private SoundItem _sound;
        private float _targetGain;

        public AreaSound(SoundType Type, Vector3 Position, float Radius)
        {
            this.Position = Position;
            this.Type = Type;
            this.Radius = Radius;
        }

        public float Radius { get; set; }
        public SoundType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Pitch { get; set; } = 1;
        public float Volume { get; set; } = 1;

        public void Dispose()
        {
            Stop();
        }

        public void Update(bool Condition)
        {
            Condition = Condition && !GameSettings.Paused;
            lock (_soundLock)
            {
                if (_sound == null && (Position - SoundPlayer.ListenerPosition).LengthFast() < Radius &&
                    Condition)
                    _sound = SoundPlayer.GetAvailableSource();

                if (_sound != null && (!_sound.Source.IsPlaying || Type != _bufferType) && Condition)
                {
                    _sound.Source.Play(SoundPlayer.GetBuffer(Type), Position, Pitch, 1, true);
                    _bufferType = Type;
                }

                if (_sound != null)
                {
                    _targetGain = Math.Max(0, 1 - (Position - SoundPlayer.ListenerPosition).LengthFast() / Radius);
                    _targetGain *= Condition ? 1 : 0;
                    _targetGain *= SoundPlayer.Volume;
                    _targetGain *= Volume;


                    _sound.Source.Volume = Mathf.Lerp(_sound.Source.Volume, _targetGain,
                        Time.IndependentDeltaTime * 8f);
                    if (Math.Abs(_sound.Source.Volume - _targetGain) < 0.05f)
                        _sound.Source.Volume = _targetGain;
                }

                if (_sound != null && (Position - SoundPlayer.ListenerPosition).LengthFast() > Radius)
                {
                    _sound.Source.Stop();
                    _sound.Locked = false;
                    _sound = null;
                }
            }
        }

        public void Stop()
        {
            lock (_soundLock)
            {
                _targetGain = 0;
                _sound?.Source.Stop();
                if (_sound != null) _sound.Locked = false;
                _sound = null;
            }
        }
    }
}