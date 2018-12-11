using System;
using Hedra.Core;
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
            if(Enabled && _player.CanInteract) _inputHandler.Update();
            HandleTerrain();
            HandleLocation();
        }

        private void HandleLocation()
        {
            if (Enabled)
            {
                var waterHeight = Physics.WaterHeight(_player.Position)-1.5f;
                var waterNormal = Physics.WaterNormalAtPosition(_player.Position);
                var heightFactor = Vector3.Dot(_player.Orientation, waterNormal);
                var boatY = _player.Physics.TargetPosition.Y + 1 * (1-heightFactor) * 0;
                OnWaterSurface = Math.Abs(boatY - waterHeight) < 0.05f;
                InWater = OnWaterSurface || boatY < waterHeight;
                if (InWater)
                {
                    _player.Physics.ResetFall();

                    /* Boat is under the surface */
                    if (!OnWaterSurface)
                    {
                        _player.Physics.TargetPosition = 
                            new Vector3(
                                _player.Physics.TargetPosition.X, 
                                Mathf.Lerp(_player.Physics.TargetPosition.Y, waterHeight + 2, Time.DeltaTime * 8f),
                                _player.Physics.TargetPosition.Z
                                );
                        OnWaterSurface = true;
                        //_player.Physics.GravityDirection = Vector3.UnitY;
                    }
                    else
                    {
                        _player.Physics.ResetVelocity();
                    }
                }
                else
                {
                    _player.Physics.GravityDirection = -Vector3.UnitY;
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

        public bool CanEnable => !_player.IsGrounded && !_player.IsRiding;
        
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if(value && !CanEnable) return;
                _enabled = value;
            }
        }

        public bool OnWaterSurface { get; private set; }

        public bool ShouldDrift => _inputHandler.ShouldDrift;

        public bool InWater { get; private set; }

        public Vector3 Velocity => _inputHandler.Velocity;
    }
}