using System;
using Hedra.Engine.Events;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatInputHandler
    {
        private const float Speed = 1.5f;
        private readonly IPlayer _player;
        private Vector3 _accumulatedDirection;
        private Vector3 _targetAngles;
        private Vector3 _angles;
        private float _yaw;
        private float _characterRotation;

        public BoatInputHandler(IPlayer Player)
        {
            _player = Player;
        }
        
        public void Update()
        {
            const float propulsionFactor = 40.0f;
            var any = GameManager.Keyboard[Key.W] || GameManager.Keyboard[Key.A] || GameManager.Keyboard[Key.D];
            if (GameManager.Keyboard[Key.W])
            {
                _accumulatedDirection = _player.View.Forward * propulsionFactor;
            }
            if (GameManager.Keyboard[Key.A])
            {
                var dir = GameManager.Keyboard[Key.W] ? (_player.View.Left + _player.View.Forward) * .5f : _player.View.Left;
                _accumulatedDirection = dir * propulsionFactor;
            }
            if (GameManager.Keyboard[Key.D])
            {
                var dir = GameManager.Keyboard[Key.W] ? (_player.View.Right + _player.View.Forward) * .5f : _player.View.Right;
                _accumulatedDirection = dir * propulsionFactor;
            }
            if (_accumulatedDirection.LengthFast > .005f)
            {
                _player.Movement.ProcessMovement(_characterRotation, _accumulatedDirection * Speed, _accumulatedDirection.LengthFast > 5f);
            }
            if (any)
            {
                this.HandleCharacterRotation();
            }
            this.HandleBoatRotation(propulsionFactor);
            _accumulatedDirection *= (float)Math.Pow(.35f, (float)Time.DeltaTime);
        }

        private void HandleBoatRotation(float MaxPropulsion)
        {
            var movingFactor = (_accumulatedDirection.LengthFast-1) / MaxPropulsion;
            _targetAngles.Z = Mathf.Clamp(15f * (_player.View.StackedYaw - _yaw) / (float)(Math.PI * .2f) * movingFactor, -45, 45);
            _angles = Mathf.Lerp(_angles, _targetAngles, Time.DeltaTime * 4f);
            _targetAngles = Mathf.Lerp(_targetAngles, Vector3.Zero, Time.DeltaTime * 4f);
            _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, (float)Time.DeltaTime * 2f);

            _player.Model.TransformationMatrix =
                Matrix4.CreateRotationY(-_player.Model.Rotation.Y * Mathf.Radian)
                * Matrix4.CreateRotationZ(_angles.Z * Mathf.Radian)
                * Matrix4.CreateRotationX(_angles.X * Mathf.Radian)
                * Matrix4.CreateRotationY(_player.Model.Rotation.Y * Mathf.Radian);
        }

        private void HandleCharacterRotation()
        {
            _characterRotation = _player.FacingDirection;
            if (GameManager.Keyboard[Key.D]) _characterRotation += -90f;
            if (GameManager.Keyboard[Key.A]) _characterRotation += 90f;
            if (GameManager.Keyboard[Key.W]) _characterRotation += 0f;
            if (GameManager.Keyboard[Key.W] && GameManager.Keyboard[Key.D]) _characterRotation += 45f;
            if (GameManager.Keyboard[Key.W] && GameManager.Keyboard[Key.A]) _characterRotation += -45f;
        }

        public bool ShouldDrift { get; private set; }

        public Vector3 Velocity => _accumulatedDirection;
    }
}