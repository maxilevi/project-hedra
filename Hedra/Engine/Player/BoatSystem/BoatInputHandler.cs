using System;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using OpenTK;
using Hedra.Engine.PhysicsSystem;
using Hedra.Game;
using Hedra.Localization;
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
        private BoatStateHandler _stateHandler;

        public BoatInputHandler(IPlayer Player, BoatStateHandler StateHandler)
        {
            _player = Player;
            _stateHandler = StateHandler;
        }
        
        public void Update()
        {
            const float propulsionFactor = 35.0f;
            var any = GameManager.Keyboard[Controls.Forward] || GameManager.Keyboard[Controls.Leftward] || GameManager.Keyboard[Controls.Rightward];
            if (GameManager.Keyboard[Controls.Forward])
            {
                var orientation = Vector3.TransformVector(_player.View.Forward, _stateHandler.Transformation);
                _accumulatedDirection = orientation * propulsionFactor;
            }
            if (GameManager.Keyboard[Controls.Leftward])
            {
                var dir = GameManager.Keyboard[Controls.Forward] ? (_player.View.Left + _player.View.Forward) * .5f : _player.View.Left;
                _accumulatedDirection = dir * propulsionFactor;
            }
            if (GameManager.Keyboard[Controls.Rightward])
            {
                var dir = GameManager.Keyboard[Controls.Forward] ? (_player.View.Right + _player.View.Forward) * .5f : _player.View.Right;
                _accumulatedDirection = dir * propulsionFactor;
            }
            if (_accumulatedDirection.LengthFast > .005f)
            {
                /* Manually translate the boat, avoid using the physics engine*/
                _player.Movement.ProcessTranslation(_characterRotation, _accumulatedDirection * Speed, _accumulatedDirection.LengthFast > 5f);
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
            _targetAngles.Z = Mathf.Clamp(45f * (_player.View.StackedYaw - _yaw) / (float)(Math.PI * .2f) * movingFactor, -45, 45);
            _angles = Mathf.Lerp(_angles, _targetAngles, Time.DeltaTime * 4f);
            _targetAngles = Mathf.Lerp(_targetAngles, Vector3.Zero, Time.DeltaTime * 4f);
            _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, (float)Time.DeltaTime * 2f);

            _player.Model.TransformationMatrix =
                Matrix4.CreateRotationY(-_player.Model.LocalRotation.Y * Mathf.Radian)
                * Matrix4.CreateRotationZ(_angles.Z * Mathf.Radian)
                * Matrix4.CreateRotationX(_angles.X * Mathf.Radian)
                * Matrix4.CreateRotationY(_player.Model.LocalRotation.Y * Mathf.Radian);
        }

        private void HandleCharacterRotation()
        {
            _characterRotation = _player.FacingDirection;
            if (GameManager.Keyboard[Controls.Rightward]) _characterRotation += -90f;
            if (GameManager.Keyboard[Controls.Leftward]) _characterRotation += 90f;
            if (GameManager.Keyboard[Controls.Forward]) _characterRotation += 0f;
            if (GameManager.Keyboard[Controls.Forward] && GameManager.Keyboard[Controls.Rightward]) _characterRotation += 45f;
            if (GameManager.Keyboard[Controls.Forward] && GameManager.Keyboard[Controls.Leftward]) _characterRotation += -45f;
        }

        public bool ShouldDrift { get; private set; }

        public Vector3 Velocity => _accumulatedDirection;
    }
}