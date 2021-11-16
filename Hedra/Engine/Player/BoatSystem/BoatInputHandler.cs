using System;
using System.Numerics;
using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Engine.Player.BoatSystem
{
    public abstract class BoatInputHandler
    {
        private readonly IHumanoid _humanoid;
        protected Vector3 _accumulatedDirection;
        protected Vector3 _angles;
        protected Vector3 _targetAngles;
        protected float _yaw;

        protected BoatInputHandler(IHumanoid Humanoid)
        {
            _humanoid = Humanoid;
        }

        protected virtual float Yaw => 0;
        public abstract bool ShouldDrift { get; }
        public abstract Vector3 Velocity { get; }

        protected void HandleBoatRotation(float MaxPropulsion)
        {
            var movingFactor = (_accumulatedDirection.LengthFast() - 1) / MaxPropulsion;
            _targetAngles.Z = Mathf.Clamp(45f * (Yaw - _yaw) / (float)(Math.PI * .2f) * movingFactor, -45, 45);
            _angles = Mathf.Lerp(_angles, _targetAngles, Time.DeltaTime * 4f);
            _targetAngles = Mathf.Lerp(_targetAngles, Vector3.Zero, Time.DeltaTime * 4f);
            _yaw = Mathf.Lerp(_yaw, Yaw, Time.DeltaTime * 2f);

            _humanoid.Model.TransformationMatrix =
                Matrix4x4.CreateRotationY(-_humanoid.Model.LocalRotation.Y * Mathf.Radian)
                * Matrix4x4.CreateRotationZ(_angles.Z * Mathf.Radian)
                * Matrix4x4.CreateRotationX(_angles.X * Mathf.Radian)
                * Matrix4x4.CreateRotationY(_humanoid.Model.LocalRotation.Y * Mathf.Radian);
        }

        public abstract void Update();
    }
}