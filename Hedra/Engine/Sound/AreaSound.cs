using System;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Game;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.Sound
{
    public class AreaSound : IDisposable
    {
        public float Radius { get; set; }
        public SoundType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Pitch { get; set; } = 1;
        public float Volume { get; set; } = 1;
        private float _targetGain;
        private SoundItem _sound;
        private SoundType _bufferType;
        private readonly object _soundLock = new object();

        public AreaSound(SoundType Type, Vector3 Position, float Radius)
        {
            this.Position = Position;
            this.Type = Type;
            this.Radius = Radius;
        }

        public void Update(bool Condition)
        {
            Condition = Condition && !GameSettings.Paused;
            lock (_soundLock)
            {
                if (this._sound == null && (this.Position - SoundPlayer.ListenerPosition).LengthFast() < Radius &&
                    Condition)
                    this._sound = SoundPlayer.GetAvailableSource();

                if (this._sound != null && (!this._sound.Source.IsPlaying || Type != _bufferType) && Condition)
                {
                    this._sound.Source.Play(SoundPlayer.GetBuffer(Type), this.Position, Pitch, 1, true);
                    _bufferType = Type;
                }

                if (this._sound != null)
                {
                    _targetGain = Math.Max(0, 1 - (this.Position - SoundPlayer.ListenerPosition).LengthFast() / Radius);
                    _targetGain *= Condition ? 1 : 0;
                    _targetGain *= SoundPlayer.Volume;
                    _targetGain *= Volume;


                    this._sound.Source.Volume = Mathf.Lerp(this._sound.Source.Volume, this._targetGain,
                        (float) Time.IndependentDeltaTime * 8f);
                    if (Math.Abs(this._sound.Source.Volume - this._targetGain) < 0.05f)
                        this._sound.Source.Volume = _targetGain;

                }

                if (this._sound != null && (this.Position - SoundPlayer.ListenerPosition).LengthFast() > Radius)
                {
                    this._sound.Source.Stop();
                    this._sound.Locked = false;
                    this._sound = null;
                }
            }
        }

        public void Stop()
        {
            lock (_soundLock)
            {
                _targetGain = 0;
                _sound?.Source.Stop();
                if (this._sound != null) this._sound.Locked = false;
                _sound = null;
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}