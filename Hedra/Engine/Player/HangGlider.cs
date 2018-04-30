using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Hedra.Engine.Rendering;
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

    public HangGlider(LocalPlayer Player)
        {
            _player = Player;
            _model = new GliderModel();
        }

        private void HandleInput()
        {
            _targetAngles.X = -45f * _player.View.Pitch;
            _targetAngles.Z = 80f * (_player.View.StackedYaw - _yaw) / (float) (Math.PI * .2f);
            _targetAngles = Mathf.Clamp(_targetAngles, -90, 90);
            _angles = Mathf.Lerp(_angles, _targetAngles, (float) Time.deltaTime * 8f);
            _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, (float) Time.deltaTime * 2f);
        }

        public void Update()
        {
            this.HandleInput();
            _model.Enabled = true;
            _model.Position = _player.Position + Vector3.UnitY * 4;
            _player.View.MaxPitch = 1.75f;
            _player.View.MinPitch = -1.75f;
            _player.Model.Enabled = false;
            _model.BaseMesh.Rotation = new Vector3(_angles.X, _player.Model.Model.Rotation.Y, 0);
            _model.BaseMesh.LocalRotation = Vector3.UnitZ * _angles.Z;
            _player.Movement.Orientate();
            _player.Physics.GravityDirection = -Vector3.UnitY * 1f;
            _player.Physics.VelocityCap = 100f * Math.Max(_player.View.Pitch, 0);
            var propulsion = Vector3.One * 120f;
            propulsion *= 1f + (_player.View.Pitch-.1f) / -2f;
            //propulsion *= propulsion.Y > 0 ? .25f : 1f;
            _accumulatedVelocity += propulsion * (float)Time.deltaTime;
            _accumulatedVelocity *= (float) Math.Pow(.375f, (float)Time.deltaTime);
            _player.View.MaxDistance = 10f;
            _player.Physics.Move(_player.View.LookingDirection * _accumulatedVelocity * (float)Time.deltaTime);
            _player.Physics.ResetFall();
            _player.Model.Glide();
        }

        public void Push(float Amount)
        {
            _accumulatedVelocity += Vector3.One * Amount;
        }

        public void Disable()
        {
            _player.View.MaxPitch = Camera.DefaultMaxPitch;
            _player.View.MinPitch = Camera.DefaultMinPitch;
            _player.Model.Enabled = true;
            _player.View.MaxDistance = Camera.DefaultMaxDistance;
            _player.Model.TargetRotation = new Vector3(0, _player.Model.TargetRotation.Y, 0);
            _model.Enabled = false;
            _player.Physics.GravityDirection = -Vector3.UnitY;
            _player.Physics.VelocityCap = float.MaxValue;
        }

        public bool Enabled
        {
            get { return _model.Enabled; }
            set { _model.Enabled = value; }
        }
    }
}
