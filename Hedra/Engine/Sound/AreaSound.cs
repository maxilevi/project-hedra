using System;
using OpenTK;

namespace Hedra.Engine.Sound
{
    internal class AreaSound
    {
        public float Radius { get; set; }
        public SoundType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Pitch { get; set; } = 1f;
        private float _targetGain;
        private SoundItem _sound;
        private SoundType _bufferType;

        public AreaSound(SoundType Type, Vector3 Position, float Radius)
        {
            this.Position = Position;
            this.Type = Type;
            this.Radius = Radius;
        }

        public void Update(bool Condition)
        {
            Condition = Condition && !GameSettings.Paused;
            if (this._sound == null && (this.Position - SoundManager.ListenerPosition).LengthFast < Radius && Condition)
                this._sound = SoundManager.GetAvailableSource();

            if (this._sound != null && (!this._sound.Source.IsPlaying || Type != _bufferType) && Condition)
            {
                this._sound.Source.Play(SoundManager.GetBuffer(Type), this.Position, Pitch, 1f, true);
                _bufferType = Type;
            }

            if (this._sound != null)
            {
                _targetGain = Math.Max(0, 1 - (this.Position - SoundManager.ListenerPosition).LengthFast / Radius);
                _targetGain *= Condition ? 1 : 0;


                this._sound.Source.Volume = Mathf.Lerp(this._sound.Source.Volume, this._targetGain, (float)Time.IndependantDeltaTime * 8f);
                if (Math.Abs(this._sound.Source.Volume - this._targetGain) < 0.05f)
                    this._sound.Source.Volume = _targetGain;

            }
            if (this._sound != null && (this.Position - SoundManager.ListenerPosition).LengthFast > Radius)
            {
                this._sound.Source.Stop();
                this._sound.Locked = false;
                this._sound = null;
            }
        }

        public void Stop()
        {
            this._targetGain = 0;
            this._sound?.Source.Stop();
            if (this._sound != null) this._sound.Locked = false;
            this._sound = null;
        }
    }
}