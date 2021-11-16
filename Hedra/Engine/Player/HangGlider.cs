using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Rendering.Particles;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.Engine.Player
{
    public class HangGlider : IVehicle, IDisposable
    {
        private readonly TrailRenderer _leftTrail;
        private readonly TrailRenderer _mainTrail;
        private readonly GliderModel _model;
        private readonly IPlayer _player;
        private readonly TrailRenderer _rightTrail;
        private Vector3 _accumulatedVelocity;
        private Vector3 _angles;
        private readonly float _decaySpeed = 20f;
        private int _remainingParticles;
        private Vector3 _targetAngles;
        private float _upPush;
        private float _yaw;

        public HangGlider(IPlayer Player)
        {
            _player = Player;
            _model = new GliderModel();
            _leftTrail = new TrailRenderer(
                () => _model.TransformPoint(Vector3.UnitZ * 4f - Vector3.UnitX * 3f - Vector3.UnitY * .25f),
                new Vector4(Vector3.One, .5f))
            {
                UpdateRate = 1,
                Orientation = Vector3.UnitZ,
                MaxLifetime = .5f
            };
            _rightTrail = new TrailRenderer(
                () => _model.TransformPoint(Vector3.UnitZ * 4f + Vector3.UnitX * 3f - Vector3.UnitY * .25f),
                new Vector4(Vector3.One, .5f))
            {
                UpdateRate = 1,
                Orientation = Vector3.UnitZ,
                MaxLifetime = .5f
            };
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs EventArgs)
            {
                if (!Enabled || !_player.CanInteract || EventArgs.Key != Controls.Jump ||
                    _player.Stamina < _player.MaxStamina * .25f) return;
                Push(220f);
                _player.Stamina -= _player.MaxStamina * .25f;
                SoundPlayer.PlaySoundWithVariation(SoundType.Jump, _player.Position);
                _remainingParticles = 20;
            });
        }

        private float ClampedPitch => Mathf.Clamp(_player.View.Pitch, -1.25f, 1.0f);

        public void Update()
        {
            HandleLanding();
            if (!Enabled)
            {
                _accumulatedVelocity = Vector3.One * 10f;
                _yaw = _player.View.StackedYaw;
            }

            _model.Enabled = _player.Model.Enabled && Enabled;
            if (Enabled)
            {
                ManageParticles();
                HandleInput();
                _player.View.MaxPitch = 1.25f;
                _player.View.MinPitch = -1.25f;

                _model.Position = _player.Model.ModelPosition + Vector3.UnitY * 8f;
                _model.BeforeRotation = Vector3.UnitY * 3.5f;
                _model.Rotation = new Vector3(_angles.X, _player.Model.LocalRotation.Y, 0);
                _model.LocalRotation = Vector3.UnitZ * _angles.Z;
                _player.Model.TransformationMatrix =
                    Matrix4x4.CreateRotationY(-_player.Model.LocalRotation.Y * Mathf.Radian)
                    * Matrix4x4.CreateTranslation(Vector3.UnitY * -7.5f)
                    * Matrix4x4.CreateRotationZ(_angles.Z * Mathf.Radian) *
                    Matrix4x4.CreateRotationX(_angles.X * Mathf.Radian)
                    * Matrix4x4.CreateRotationY(_player.Model.LocalRotation.Y * Mathf.Radian)
                    * Matrix4x4.CreateTranslation(Vector3.UnitY * 10f);
                _player.Movement.Orientate();
                _player.Physics.GravityDirection = -Vector3.UnitY * 1f;
                /*_player.Physics.VelocityCap = 160f * Math.Max(ClampedPitch, 0) + 1f *
                                              (1.0f - Math.Min(1f,
                                                   (_accumulatedVelocity.Average() - _decaySpeed) / _decaySpeed)) + 15f
                                              + 20f * Math.Max(0, _angles.Z / 90f);*/
                var propulsion = Vector3.One * 20f;
                propulsion *= 1f + _angles.X / 45f;
                propulsion *= _angles.X < 15f ? 1.4f : 4.0f;
                _accumulatedVelocity += propulsion * Time.DeltaTime;
                _accumulatedVelocity *= (float)Math.Pow(.8f, Time.DeltaTime);
                _upPush *= (float)Math.Pow(.25f, Time.DeltaTime);
                _player.View.MaxDistance = 10f;
                _player.Physics.DeltaTranslate(
                    (_player.View.LookingDirection * _accumulatedVelocity + Vector3.UnitY * _upPush) * .55f);
                _player.Physics.ResetFall();

                _leftTrail.Thickness = 1f * (Math.Abs(_angles.Z) - 15f) / 90f *
                                       Math.Min(1f, _accumulatedVelocity.Average() / _decaySpeed);
                _rightTrail.Thickness = 1f * (Math.Abs(_angles.Z) - 15f) / 90f *
                                        Math.Min(1f, _accumulatedVelocity.Average() / _decaySpeed);

                _leftTrail.Emit = _leftTrail.Thickness > 0.05f && _model.Enabled;
                _rightTrail.Emit = _rightTrail.Thickness > 0.05f && _model.Enabled;
            }

            _rightTrail.Update();
            _leftTrail.Update();
        }

        public void Disable()
        {
            _player.View.MaxPitch = Camera.DefaultMaxPitch;
            _player.View.MinPitch = Camera.DefaultMinPitch;
            _player.View.MaxDistance = Camera.DefaultMaxDistance;
            _player.Model.TransformationMatrix = Matrix4x4.Identity;
            _player.Model.Pause = false;
            _player.Physics.GravityDirection = -Vector3.UnitY;
            _leftTrail.Emit = false;
            Enabled = false;
        }

        public void Enable()
        {
            Enabled = true;
        }

        public bool CanEnable => !_player.IsGrounded;

        public bool Enabled { get; private set; }

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
        }

        private void HandleInput()
        {
            _targetAngles.X = -45f * ClampedPitch +
                              45f * (1.0f - Math.Min(1f, (_accumulatedVelocity.Average() - _decaySpeed) / _decaySpeed));
            _targetAngles.Z = 45f * (_player.View.StackedYaw - _yaw) / (float)(Math.PI * .2f);
            _targetAngles = Mathf.Clamp(_targetAngles, -90, 90);
            _angles = Mathf.Lerp(_angles, _targetAngles, Time.DeltaTime * 4f);
            _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, Time.DeltaTime * 2f);
        }

        private void HandleLanding()
        {
            if (Enabled)
            {
                if (_player.IsGrounded)
                {
                    Disable();
                    _player.AddBonusSpeedForSeconds(-_player.Speed + _player.Speed * .5f, 2f);
                }
                else if (_player.IsUnderwater)
                {
                    Disable();
                }
            }
        }

        public void Push(float Amount)
        {
            _upPush += Amount;
        }

        private void ManageParticles()
        {
            if (_remainingParticles > 0)
            {
                World.Particles.Position = _player.Model.Position;
                World.Particles.Color = Vector4.One;
                World.Particles.Direction = -Vector3.UnitY * 4f;
                World.Particles.Scale = Vector3.One * .25f;
                World.Particles.PositionErrorMargin = Vector3.Zero;
                World.Particles.Emit();

                _remainingParticles--;
            }
        }
    }
}