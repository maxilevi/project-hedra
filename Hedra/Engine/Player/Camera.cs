using System;
using System.Drawing;
using System.Windows.Forms;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public class Camera : EventListener, ICamera
    {
        public static Vector3 DefaultCameraHeight = Vector3.UnitY * 9.0f;
        public static Func<Vector3> DefaultDelegate;
        public const float DefaultMaxDistance = 12f;
        public const float DefaultMinDistance = 2.0f;
        public const float DefaultMaxPitch = 1.5f;
        public const float DefaultMinPitch = -2f;

        public float TargetPitch { get; set; }
        public float TargetYaw { get; set; }
        public float MaxDistance { get; set; } = DefaultMaxDistance;
        public float MinDistance { get; set; } = DefaultMinDistance;
        public float MaxPitch { get; set; } = DefaultMaxPitch;
        public float MinPitch { get; set; } = DefaultMinPitch;
        public float AddonDistance { get; set; }
        public float Distance { get; set; }
        public float WheelSpeed { get; set; } = 1;
        public bool CaptureMovement { get; set; } = true;
        public bool LockMouse { get; set; } = true;
        public Matrix4 ModelViewMatrix { get; private set; }
        public Func<Vector3> PositionDelegate { get; set; }
        public int XDelta { get; private set; }
        public int YDelta { get; private set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float StackedYaw { get; private set; }

        private Vector3 _cameraHeight = Vector3.UnitY * 12.0f;
        private readonly LocalPlayer _player;
        private float _previousAlpha = -1f;
        private float _prevDistance;
        private Vector3 _interpolatedPosition;
        private Vector3 _lastDelegateValue;
        private Vector3 _interpolatedZoomOut;
        private Vector3 _targetZoomOut;

        public Camera(LocalPlayer RefPlayer)
        {
            _player = RefPlayer;

            if (Camera.DefaultDelegate == null)
            {
                Camera.DefaultDelegate = () => _player.Model.ModelPosition;
            }
            this.PositionDelegate = Camera.DefaultDelegate;
        }

        public void Update()
        {
            this.Interpolate();
            this.ClampYaw();

            if ( !GameSettings.Paused && !GameManager.IsLoading && !_player.IsDead)
            {
                XDelta = Cursor.Position.X - GameSettings.Width / 2;
                YDelta = Cursor.Position.Y - GameSettings.Height / 2;
                if (LockMouse) Cursor.Position = new Point(GameSettings.Width / 2, GameSettings.Height / 2);
                
                if (CaptureMovement)
                {
                    this.ManageRotations();
                }

                Vector3 pos = CameraPosition;
                float y = Physics.HeightAtPosition(pos), addonDistance = 0;
                if (pos.Y <= y + MinDistance ||
                    Physics.IsColliding(pos, new Box(-Vector3.One * 2f + pos, Vector3.One * 2f + pos)))
                {
                    if (_prevDistance == 0)
                        _prevDistance = TargetDistance;
                    TargetDistance += Time.IndependantDeltaTime * -24f;
                }
                else
                {
                    this.ClampDistance();
                }

                this.ManageAlpha();
            }
            this.BuildCameraMatrix();
            DrawManager.FrustumObject.CalculateFrustum(DrawManager.FrustumObject.ProjectionMatrix, ModelViewMatrix);
            DrawManager.FrustumObject.SetFrustum(ModelViewMatrix);
        }

        private void ManageRotations()
        {
            TargetYaw += XDelta * GameSettings.MouseSensibility * 0.0025f;
            StackedYaw += XDelta * GameSettings.MouseSensibility * 0.0025f;

            float possiblePitch = (!GameSettings.InvertMouse ? -YDelta : YDelta) * GameSettings.MouseSensibility * 0.0025f;
            Vector3 camPos =
                _interpolatedPosition - new Vector3((float)Math.Cos(TargetYaw), TargetPitch + possiblePitch, (float)Math.Sin(TargetYaw)) *
                new Vector3(TargetDistance, TargetDistance, TargetDistance) + CameraHeight;

            float heightAtCamPos = Physics.HeightAtPosition(camPos);
            if (camPos.Y > heightAtCamPos + MinDistance) TargetPitch += possiblePitch;

            TargetPitch = Mathf.Clamp(TargetPitch, MinPitch, MaxPitch);
        }

        private void Interpolate()
        {
            _targetZoomOut = _player.IsMoving && !_player.IsUnderwater ? _player.Orientation * 2.0f : Vector3.Zero;
            if (DefaultDelegate == PositionDelegate)
            {
                _targetZoomOut += _player.IsJumping ? Vector3.UnitY * 2f : Vector3.Zero;
            }
            _interpolatedZoomOut = Mathf.Lerp(_interpolatedZoomOut, _targetZoomOut, Time.DeltaTime * 2f);
            _interpolatedPosition = Mathf.Lerp(_interpolatedPosition, PositionDelegate() - _interpolatedZoomOut, Time.IndependantDeltaTime * 16f);
            Pitch = Mathf.Lerp(Pitch, TargetPitch, Time.IndependantDeltaTime * 16f);
            Yaw = Mathf.Lerp(Yaw, TargetYaw, Time.IndependantDeltaTime * 16f);
            var cameraPosition = this.CalculatePosition(0);
            var addonDistance = cameraPosition.Y > Physics.HeightAtPosition(cameraPosition) + 2 ? AddonDistance : 0;
            Distance = Mathf.Lerp(Distance, TargetDistance + addonDistance * Time.TimeScale, Time.IndependantDeltaTime * 3f);
        }

        private void ClampYaw()
        {
            if (TargetYaw > Math.PI * 2f)
            {
                TargetYaw -= (float) Math.PI * 2f;
                Yaw -= (float)Math.PI * 2f;
            }
            if (TargetYaw < -Math.PI * 2f)
            {
                TargetYaw += (float) Math.PI * 2f;
                Yaw += (float)Math.PI * 2f;
            }
        }

        private void ManageAlpha()
        {
            TargetDistance = Mathf.Clamp(TargetDistance, 1.5f, MaxDistance);
            if (TargetDistance < 4.5f)
            {
                if (_previousAlpha <= -0.995) _previousAlpha = _player.Model.Alpha;
                _player.Model.Alpha = Mathf.Clamp((TargetDistance - 1.5f) / 4.5f, 0, 1) + 0.0025f;
            }
            else
            {
                if (Math.Abs(_previousAlpha - -1f) < 0.005f) return;
                _player.Model.Alpha = _previousAlpha;
                _previousAlpha = -1;
            }
        }

        public void BuildCameraMatrix()
        {
            ModelViewMatrix = Matrix4.LookAt(_interpolatedPosition - LookAtPoint * Distance + CameraHeight,
                _interpolatedPosition + LookAtPoint * Distance + CameraHeight, Vector3.UnitY);
        }

        public override void OnMouseWheel(object Sender, MouseWheelEventArgs E)
        {
            if (GameSettings.Paused || !CaptureMovement) return;

            Vector3 pos = _interpolatedPosition - LookAtPoint * (TargetDistance - E.Delta * WheelSpeed) + CameraHeight;
            float y = Physics.HeightAtPosition(pos);
            if (pos.Y <= y + MinDistance) return;

            TargetDistance -= E.Delta * WheelSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, 1.5f, MaxDistance);

            _prevDistance = TargetDistance;
        }

        private void ClampDistance()
        {
            if (_prevDistance <= 0.005f || !(TargetDistance < _prevDistance)) return;

            for (var i = 2; i < 5; i++)
            {
                var position = this.CalculatePosition(i) + Vector3.UnitY;
                float y = Physics.HeightAtPosition(position);
                var box = new Box(-Vector3.One * 2f + this.CalculatePosition(i-1), Vector3.One * 2f + position);
                if (position.Y <= y + MinDistance || Physics.IsColliding(position, box))
                    return;
            }
            TargetDistance += Time.IndependantDeltaTime * 24f;
        }

        private float _targetDistance = 10f;

        public float TargetDistance
        {
            get => _targetDistance;
            set
            {
                _targetDistance = value;
                if (TargetDistance < 4.5f)
                {
                    var newAlpha = Mathf.Clamp((TargetDistance - 1.5f) / 4.5f, 0, 1);
                    _player.Model.Alpha = newAlpha + 0.0025f;
                }
                else
                {
                    _player.Model.Alpha = 1;
                }
            }
        }

        private Vector3 CalculatePosition(float i)
        {
            return _interpolatedPosition + CameraHeight -
                LookAtPoint * (TargetDistance + (_prevDistance - TargetDistance) * (1f / (i + 1)));
        }

        private Vector3 LookAtPoint => new Vector3(Forward.X, Pitch, Forward.Z);

        public Vector3 LookingDirection => LookAtPoint.NormalizedFast();

        public Vector3 Forward => new Vector3((float)Math.Cos(Yaw), 0, (float)Math.Sin(Yaw));

        public Vector3 Backward => -Forward;

        public Vector3 Right => new Vector3((float)Math.Cos(Yaw + Math.PI / 2), 0, (float)Math.Sin(Yaw + Math.PI / 2));

        public Vector3 Left => -Right;

        public Vector3 CameraHeight
        {
            get
            {
                if (_player.IsSitting || _player.IsSleeping) return _cameraHeight - Vector3.UnitY * 2.0f;
                return _cameraHeight;
            }
            set => _cameraHeight = value;
        }

        public Vector3 CameraPosition => _interpolatedPosition - LookAtPoint * new Vector3(TargetDistance, TargetDistance, TargetDistance) + CameraHeight;

        public Vector3 CrossDirection
        {
            get
            {
                var lookingDir = new Vector4(0f, 0f, -1.0f, 1.0f);
                lookingDir = Vector4.Transform(lookingDir, DrawManager.FrustumObject.ProjectionMatrix.Inverted());
                lookingDir = new Vector4(lookingDir.X, lookingDir.Y, -1f, 0f);
                lookingDir = Vector4.Transform(lookingDir, DrawManager.FrustumObject.ModelViewMatrix.Inverted());
                return lookingDir.Xyz.NormalizedFast();
            }
        }
        public Vector3 CrossPosition => throw new NotImplementedException();

        public Matrix4 ViewMatrix => Matrix4.LookAt(-LookAtPoint * TargetDistance + CameraHeight,
            LookAtPoint * TargetDistance + CameraHeight,
            Vector3.UnitY);
    }
}