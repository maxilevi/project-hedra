using System;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatStateHandler
    {
        private readonly BoatInputHandler _inputHandler;
        private readonly IPlayer _player;
        private bool _wasEnabled;
        
        public BoatStateHandler(IPlayer Player)
        {
            _player = Player;
            _inputHandler = new BoatInputHandler(_player, this);
        }

        public void Update()
        {
            _inputHandler.Update();
            HandleTerrain();
            HandleLocation();
        }

        private void HandleLocation()
        {
            if (Enabled)
            {
                var waterHeight = Physics.WaterHeightAtPosition(_player.Position);
                _player.Physics.TargetPosition 
                    = new Vector3(_player.Physics.TargetPosition.X, waterHeight - .5f, _player.Physics.TargetPosition.Z);
            }
        }

        public bool CanEnable()
        {
            var waterHeight = Physics.WaterHeightAtPosition(_player.Position);
            if (Math.Abs(_player.Physics.TargetPosition.Y - waterHeight) > 8) return false;
            return !_player.IsGrounded;
        }

        private void HandleTerrain()
        {
            if (_player.IsGrounded) _inputHandler.Enabled = false;
            if (Enabled)
            {
                _player.Physics.GravityDirection = Vector3.Zero;
                _player.IsSailing = true;
                _player.Movement.CaptureMovement = false;
            }
            if (!Enabled && _wasEnabled)
            {
                _player.IsSailing = false;
                _player.Physics.GravityDirection = -Vector3.UnitY;
                _player.Movement.CaptureMovement = true;
            }
            _wasEnabled = Enabled;
        }

        public bool Enabled => _inputHandler.Enabled;
    }
}