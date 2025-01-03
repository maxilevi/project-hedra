using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.UI
{
    public class SlingShotAnimation : TextureAnimation<ISimpleTexture>
    {
        private float _force;
        private float _targetForce;
        public float Duration { get; set; } = 1;

        protected override void Start(ISimpleTexture Texture, TextureState State)
        {
            _targetForce = 1;
            base.Start(Texture, State);
        }

        protected override void Process(ISimpleTexture Texture, TextureState State)
        {
            var target = _targetForce * State.Scale.Y * 1.5f;
            _force = Mathf.Lerp(_force, target, Time.DeltaTime / Duration * 12f);
            Texture.Position = State.Position + Vector2.UnitY * _force;
            if (Math.Abs(_force - target) < 0.005f)
            {
                if (_targetForce < 0.005f) Stop();
                if (_targetForce >= 1f) _targetForce = 0;
            }
        }
    }
}