using System;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public class HangGlider
    {
        private readonly LocalPlayer _player;
        private readonly GliderModel _model;
        private Vector3 _accumulatedVelocity;
        private Vector3 _angles;
        private Vector3 _targetAngles;
        private float _yaw;
        private float ClampedPitch => Mathf.Clamp(_player.View.Pitch, -1.25f, 1.0f);
        private readonly TrailRenderer _rightTrail;
        private readonly TrailRenderer _leftTrail;
        private readonly TrailRenderer _mainTrail;
        private float _decaySpeed = 20f;
        private float _upPush;
        private int _remainingParticles;

        public HangGlider(LocalPlayer Player)
        {
            _player = Player;
            _model = new GliderModel();
            _leftTrail = new TrailRenderer(() => _model.BaseMesh.TransformPoint(Vector3.UnitZ * 4f - Vector3.UnitX * 3f - Vector3.UnitY * .25f),
                new Vector4(Vector3.One, .5f))
            {
                UpdateRate = 1,
                Orientation = Vector3.UnitZ,
                MaxLifetime = .5f
            };
            _rightTrail = new TrailRenderer(() => _model.BaseMesh.TransformPoint(Vector3.UnitZ * 4f + Vector3.UnitX * 3f - Vector3.UnitY * .25f),
                new Vector4(Vector3.One, .5f))
            {
                UpdateRate = 1,
                Orientation = Vector3.UnitZ,
                MaxLifetime = .5f
            };
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyboardKeyEventArgs EventArgs)
            {
                if (!this.Enabled || !this._player.CanInteract || EventArgs.Key != Key.Space || _player.Stamina < _player.MaxStamina * .25f) return;
                this.Push(220f);
                this._player.Stamina -= _player.MaxStamina * .25f;
                SoundManager.PlaySoundWithVariation(SoundType.Jump, _player.Position);
                _remainingParticles = 20;
            });
        }

        private void HandleInput()
        {
            _targetAngles.X = -45f * ClampedPitch + 45f * (1.0f - Math.Min(1f, (_accumulatedVelocity.Average() - _decaySpeed) / _decaySpeed));
            _targetAngles.Z = 45f * (_player.View.StackedYaw - _yaw) / (float) (Math.PI * .2f);
            _targetAngles = Mathf.Clamp(_targetAngles, -90, 90);
            _angles = Mathf.Lerp(_angles, _targetAngles, (float) Time.deltaTime * 4f);
            _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, (float) Time.deltaTime * 2f);
        }

        public void Update()
        {
            if (!_model.Enabled)
            {
                _accumulatedVelocity = Vector3.One * 10f;
                _yaw = _player.View.StackedYaw;
            }
            if (this.Enabled)
            {
                this.ManageParticles();
                this.HandleInput();
                _model.Enabled = true;
                _player.View.MaxPitch = 1.25f;
                _player.View.MinPitch = -1.25f;

                _model.Position = _player.Position + Vector3.UnitY * 8f;
                _model.BaseMesh.BeforeLocalRotation = Vector3.UnitY * 2f;
                _model.BaseMesh.Rotation = new Vector3(_angles.X, _player.Model.Model.Rotation.Y, 0);
                _model.BaseMesh.LocalRotation = Vector3.UnitZ * _angles.Z;
                _player.Model.Model.TransformationMatrix =
                    Matrix4.CreateRotationY(-_player.Model.Model.Rotation.Y * Mathf.Radian)
                    * Matrix4.CreateTranslation(Vector3.UnitY * -7.5f)
                    * Matrix4.CreateRotationZ(_angles.Z * Mathf.Radian) *
                    Matrix4.CreateRotationX(_angles.X * Mathf.Radian)
                    * Matrix4.CreateRotationY(_player.Model.Model.Rotation.Y * Mathf.Radian)
                    * Matrix4.CreateTranslation(Vector3.UnitY * 10f);
                _player.Movement.Orientate();
                _player.Physics.GravityDirection = -Vector3.UnitY * 1f;
                _player.Physics.VelocityCap = 160f * Math.Max(ClampedPitch, 0) + 10f *
                                              (1.0f - Math.Min(1f,
                                                   (_accumulatedVelocity.Average() - _decaySpeed) / _decaySpeed)) + 15f
                                              + 100f * Math.Max(0, _angles.Z / 90f);
                var propulsion = Vector3.One * 20f;
                propulsion *= 1f + _angles.X / 45f;
                propulsion *= _angles.X < 15f ? 1.4f : 4.0f;
                _accumulatedVelocity += propulsion * (float) Time.deltaTime;
                _accumulatedVelocity *= (float) Math.Pow(.8f, (float) Time.deltaTime);
                _upPush *= (float) Math.Pow(.25f, (float) Time.deltaTime);
                _player.View.MaxDistance = 10f;
                _player.Physics.Move((_player.View.LookingDirection * _accumulatedVelocity + Vector3.UnitY * _upPush) *
                                     (float) Time.deltaTime * .55f);
                _player.Physics.ResetFall();
                _player.Model.Glide();

                _leftTrail.Thickness = 1f * (Math.Abs(_angles.Z) - 15f) / 90f *
                                       Math.Min(1f, _accumulatedVelocity.Average() / _decaySpeed);
                _rightTrail.Thickness = 1f * (Math.Abs(_angles.Z) - 15f) / 90f *
                                        Math.Min(1f, _accumulatedVelocity.Average() / _decaySpeed);

                _leftTrail.Emit = _leftTrail.Thickness > 0.05f;
                _rightTrail.Emit = _rightTrail.Thickness > 0.05f;
            }

            _rightTrail.Update();
            _leftTrail.Update();
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

        public void Disable()
        {
            _player.View.MaxPitch = Camera.DefaultMaxPitch;
            _player.View.MinPitch = Camera.DefaultMinPitch;
            _player.View.MaxDistance = Camera.DefaultMaxDistance;
            _player.Model.Model.TransformationMatrix = Matrix4.Identity;
            _model.Enabled = false;
            _player.Model.Pause = false;
            _player.Physics.GravityDirection = -Vector3.UnitY;
            _player.Physics.VelocityCap = float.MaxValue;
            _leftTrail.Emit = false;
        }

        public bool Enabled
        {
            get { return _model.Enabled; }
            set { _model.Enabled = value; }
        }
    }
}
