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
        private bool _wasInWater;
        private Quaternion _targetTerrainOrientation;
        private Quaternion _terrainOrientation;


        public BoatStateHandler(IPlayer Player)
        {
            _player = Player;
            _inputHandler = new BoatInputHandler(_player, this);
        }

        public void Update()
        {
            if (Enabled && _player.CanInteract)
            {
                HandleDirection();
                _inputHandler.Update();
            }
            HandleTerrain();
            HandleLocation();
        }

        private void HandleLocation()
        {
            if (Enabled)
            {
                var waterHeight = Physics.WaterHeight(_player.Position);
                var waterNormal = Physics.WaterNormalAtPosition(_player.Position);
                var boatY = _player.Position.Y;
                OnWaterSurface = Math.Abs(boatY - waterHeight) < 0.25f;
                InWater = OnWaterSurface || boatY < waterHeight;
                if (InWater)
                {
                    _player.Physics.ResetFall();

                    /* Boat is under the surface */
                    if (!OnWaterSurface)
                    {
                        OnWaterSurface = true;
                        _player.Physics.GravityDirection = Vector3.UnitY * 40f;
                    }
                    else if(_wasInWater)
                    {
                        _player.Physics.ResetVelocity();
                    }
                }
                else
                {
                    _player.Physics.GravityDirection = -Vector3.UnitY;
                }
            }
            _wasInWater = InWater;
        }

        private void HandleDirection()
        {
            var waterNormal = OnWaterSurface || _player.IsUnderwater ? Physics.WaterNormalAtPosition(_player.Model.ModelPosition) : Vector3.UnitY;
            _targetTerrainOrientation =
                new Matrix3(Mathf.RotationAlign(Vector3.UnitY, waterNormal)).ExtractRotation();
            _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation,
                Time.IndependentDeltaTime * 1f);
            Transformation = Matrix4.CreateFromQuaternion(_terrainOrientation);
        }
        
        private void HandleTerrain()
        {
            if (_player.IsGrounded && Enabled) Enabled = false;
            if (Enabled)
            {
                _player.Movement.CaptureMovement = false;
            }
            if (!Enabled && _wasEnabled)
            {
                _player.Physics.GravityDirection = _player.IsUnderwater ? Vector3.Zero : -Vector3.UnitY;
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

        public Matrix4 Transformation { get; private set; }

        public bool OnWaterSurface { get; private set; }

        public bool ShouldDrift => _inputHandler.ShouldDrift;

        public bool InWater { get; private set; }

        public Vector3 Velocity => _inputHandler.Velocity;
    }
}