using System;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatStateHandler
    {
        private readonly BoatInputHandler _inputHandler;
        private readonly IPlayer _player;
        private bool _wasEnabled;
        private bool _enabled;
        
        public BoatStateHandler(IPlayer Player)
        {
            _player = Player;
            _inputHandler = new BoatInputHandler(_player);
        }

        public void Update()
        {
            if(Enabled) _inputHandler.Update();
            HandleTerrain();
            HandleLocation();
        }

        private void HandleLocation()
        {
            if (Enabled)
            {
                var movement = GetWaterMovement(_player.Physics.TargetPosition.X, _player.Physics.TargetPosition.Z);
                var waterHeight = Physics.WaterHeightAtPosition(_player.Position) - 1.0f;
                var boatY = _player.Physics.TargetPosition.Y;

                if (Math.Abs(boatY - waterHeight) < 1.5f)
                {
                    _player.Physics.GravityDirection = Vector3.Zero;
                    _player.Physics.Velocity *= .98f;
                    _player.Physics.TargetPosition
                        = new Vector3(_player.Physics.TargetPosition.X, waterHeight + movement, _player.Physics.TargetPosition.Z);
                    InWater = true;
                }
                else if (boatY < waterHeight)
                {
                    _player.Physics.GravityDirection = Vector3.UnitY;
                    InWater = true;
                }
                else if (boatY > waterHeight)
                {
                    _player.Physics.GravityDirection = -Vector3.UnitY;
                    InWater = false;
                }
            }
        }

        private void HandleTerrain()
        {
            if (_player.IsGrounded) Enabled = false;
            if (Enabled)
            {
                _player.Movement.CaptureMovement = false;
            }
            if (!Enabled && _wasEnabled)
            {
                _player.Physics.GravityDirection = -Vector3.UnitY;
                _player.Movement.CaptureMovement = true;
                _player.Physics.ResetFall();
            }
            _wasEnabled = Enabled;
        }

        private float GetWaterMovement(float X, float Z)
        {   
            const float waveLength = 1.75f;
            var rX = ((X + Z * X * 0.1) % waveLength / waveLength + WorldRenderer.WaveMovement * 0.2) * 2.0 * Math.PI;
            var rZ = (0.3 * (Z * X + X * Z) % waveLength / waveLength + WorldRenderer.WaveMovement * 0.2 * 2.0) * 2.0 * Math.PI;

            return (float) (1.4 * 0.5 * (Math.Sin(rX) + Math.Sin(rZ)));
            
        }

        public bool CanEnable => !_player.IsGrounded;
        
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if(value && !CanEnable) return;
                _enabled = value;
            }
        }

        public bool ShouldDrift => _inputHandler.ShouldDrift;

        public bool InWater { get; private set; }

        public Vector3 Velocity => _inputHandler.Velocity;
    }
}