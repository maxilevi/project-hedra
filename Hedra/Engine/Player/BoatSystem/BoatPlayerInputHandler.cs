using System;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using OpenToolkit.Mathematics;
using Hedra.Engine.PhysicsSystem;
using Hedra.Game;
using Hedra.Localization;


namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatPlayerInputHandler : BoatInputHandler

    {
        private const float Speed = 1.5f;
        private readonly IPlayer _player;
        private float _characterRotation;
        private BoatStateHandler _stateHandler;

        public BoatPlayerInputHandler(IPlayer Player, BoatStateHandler StateHandler) : base(Player)
        {
            _player = Player;
            _stateHandler = StateHandler;
        }

        public override void Update()
        {
            const float propulsionFactor = 35.0f;
            var any = GameManager.Keyboard[Controls.Forward] || GameManager.Keyboard[Controls.Leftward] ||
                      GameManager.Keyboard[Controls.Rightward];
            if (GameManager.Keyboard[Controls.Forward])
            {
                var orientation = Vector3.TransformVector(_player.View.Forward, _stateHandler.Transformation);
                _accumulatedDirection = orientation * propulsionFactor;
            }

            if (GameManager.Keyboard[Controls.Leftward])
            {
                var dir = GameManager.Keyboard[Controls.Forward]
                    ? (_player.View.Left + _player.View.Forward) * .5f
                    : _player.View.Left;
                _accumulatedDirection = dir * propulsionFactor;
            }

            if (GameManager.Keyboard[Controls.Rightward])
            {
                var dir = GameManager.Keyboard[Controls.Forward]
                    ? (_player.View.Right + _player.View.Forward) * .5f
                    : _player.View.Right;
                _accumulatedDirection = dir * propulsionFactor;
            }

            if (_accumulatedDirection.LengthFast > .005f)
            {
                /* Manually translate the boat, avoid using the physics engine*/
                _player.Movement.ProcessTranslation(_characterRotation, _accumulatedDirection * Speed,
                    _accumulatedDirection.LengthFast > 5f);
            }

            if (any)
            {
                this.HandleCharacterRotation();
            }

            this.HandleBoatRotation(propulsionFactor);
            _accumulatedDirection *= (float) Math.Pow(.35f, (float) Time.DeltaTime);
        }

        private void HandleCharacterRotation()
        {
            _characterRotation = _player.FacingDirection;
            if (GameManager.Keyboard[Controls.Rightward]) _characterRotation += -90f;
            if (GameManager.Keyboard[Controls.Leftward]) _characterRotation += 90f;
            if (GameManager.Keyboard[Controls.Forward]) _characterRotation += 0f;
            if (GameManager.Keyboard[Controls.Forward] && GameManager.Keyboard[Controls.Rightward])
                _characterRotation += 45f;
            if (GameManager.Keyboard[Controls.Forward] && GameManager.Keyboard[Controls.Leftward])
                _characterRotation += -45f;
        }

        protected override float Yaw => _player.View.StackedYaw;
        public override bool ShouldDrift { get; }

        public override Vector3 Velocity => _accumulatedDirection;
    }
}