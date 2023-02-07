using System;
using System.Numerics;
using BulletSharp;
using Hedra.Core;
using Hedra.Engine.Bullet;
using Hedra.Engine.Events;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Input;
using Hedra.Numerics;

namespace Hedra.Engine.Player
{
    public class Camera : EventListener, ICamera
    {
        private const float DefaultDistance = DefaultMaxDistance;
        public const float DefaultMaxDistance = 24f;
        public const float DefaultMinDistance = 2.0f;
        public const float DefaultMaxPitch = 1.5f;
        public const float DefaultMinPitch = -2f;
        private const float DistanceBuffer = 1.75f;
        public static Vector3 DefaultCameraHeight = Vector3.UnitY * 8.0f;
        public static Func<Vector3> DefaultDelegate;

        private readonly ClosestRayResultCallback _callback;
        private readonly IPlayer _player;
        private Vector3 _interpolatedPosition;
        private Vector3 _interpolatedZoomOut;
        private Vector3 _lastDelegateValue;
        private Vector2 _lastMousePosition;
        private float _previousAlpha = -1f;
        private Vector3 _targetZoomOut;
        private float _wheelDelta;
        private float _xDelta;
        private float _yDelta;

        public Camera(IPlayer Player)
        {
            _player = Player;
            var src = BulletSharp.Math.Vector3.Zero;
            var dst = BulletSharp.Math.Vector3.Zero;
            _callback = new ClosestRayResultCallback(ref src, ref dst);
            Reset();
        }

        private Vector3 LookAtPoint => new Vector3(Forward.X, Pitch, Forward.Z);

        private Vector3 CameraLookAtPosition => _interpolatedPosition + CameraOrientation + CameraHeight;

        private Vector3 CameraOrientation => GetCameraOrientation();

        public float TargetPitch { get; set; }
        public float TargetYaw { get; set; }
        public float MaxDistance { get; set; }
        public float MinDistance { get; set; }
        public float MaxPitch { get; set; }
        public float MinPitch { get; set; }
        public float Distance { get; set; }
        public float WheelSpeed { get; set; }
        public bool CaptureMovement { get; set; }
        public float AddedDistance { get; set; }
        public bool LockMouse { get; set; }
        public bool AllowClipping { get; set; }
        public Matrix4x4 ModelViewMatrix { get; private set; }
        public Func<Vector3> PositionDelegate { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float StackedYaw { get; private set; }

        public void Reset()
        {
            if (DefaultDelegate == null) DefaultDelegate = () => _player.Model.ModelPosition;
            PositionDelegate = DefaultDelegate;
            WheelSpeed = 1;
            CaptureMovement = true;
            LockMouse = true;
            MaxDistance = DefaultMaxDistance;
            MinDistance = DefaultMinDistance;
            MaxPitch = DefaultMaxPitch;
            MinPitch = DefaultMinPitch;
            CameraHeight = DefaultCameraHeight;
        }

        public void Update()
        {
            Interpolate();
            ClampYaw();

            if (!GameSettings.Paused && !GameManager.IsLoading && !_player.IsDead && !GameSettings.ContinousMove)
            {
                var position = Cursor.Position;
                _xDelta = position.X - GameSettings.Width / 2f;
                _yDelta = position.Y - GameSettings.Height / 2f;
                if (LockMouse) Cursor.Center();

                if (CaptureMovement) ManageRotations();
            }

            bool ShouldMove(out float NewDistance)
            {
                return IsColliding(GetCameraEyePosition(), out NewDistance) ||
                       IsColliding(GetCameraEyePosition(Distance + DistanceBuffer), out NewDistance);
            }

            var entered = false;
            while (!GameManager.IsLoading && !GameManager.InStartMenu && TargetDistance > MinDistance &&
                   ShouldMove(out var newDistance))
            {
                TargetDistance -= Time.IndependentDeltaTime;
                Distance -= Time.IndependentDeltaTime;
                entered = true;
            }

            BuildCameraMatrix();
            Culling.BuildFrustum(ModelViewMatrix);
        }


        public void BuildCameraMatrix()
        {
            ModelViewMatrix = Matrix4x4.CreateLookAt(CameraEyePosition, CameraLookAtPosition, Vector3.UnitY);
        }

        public float TargetDistance { get; set; } = DefaultDistance;

        public Vector3 LookingDirection => LookAtPoint.NormalizedFast();

        public Vector3 Forward => new Vector3((float)Math.Cos(Yaw), 0, (float)Math.Sin(Yaw));

        public Vector3 Backward => -Forward;

        public Vector3 Right => new Vector3((float)Math.Cos(Yaw + Math.PI / 2), 0, (float)Math.Sin(Yaw + Math.PI / 2));

        public Vector3 Left => -Right;

        public Vector3 CameraHeight { get; set; }

        public Vector3 CameraEyePosition => GetCameraEyePosition();

        public Vector3 CrossDirection
        {
            get
            {
                var lookingDir = new Vector4(0f, 0f, -1.0f, 1.0f);
                lookingDir = Vector4.Transform(lookingDir, Culling.ProjectionMatrix.Inverted());
                lookingDir = new Vector4(lookingDir.X, lookingDir.Y, -1f, 0f);
                lookingDir = Vector4.Transform(lookingDir, Culling.ModelViewMatrix.Inverted());
                return lookingDir.Xyz().NormalizedFast();
            }
        }

        private void ManageRotations()
        {
            TargetYaw += _xDelta * GameSettings.MouseSensibility * 0.0025f;
            StackedYaw += _xDelta * GameSettings.MouseSensibility * 0.0025f;

            var possiblePitch = (!GameSettings.InvertMouse ? -_yDelta : _yDelta) * GameSettings.MouseSensibility *
                                0.0025f;
            TargetPitch = Mathf.Clamp(TargetPitch + possiblePitch, MinPitch, MaxPitch);
            if (Math.Abs(TargetPitch - MinPitch) < 0.005f)
            {
                //TargetPitch += Time.IndependentDeltaTime * 4f;
                //Pitch += Time.IndependentDeltaTime * 8f;
                //TargetDistance = Math.Max(TargetDistance - Time.IndependentDeltaTime * 16f, MinDistance);
                //Distance = Math.Max(Distance - Time.IndependentDeltaTime * 24f, MinDistance);
            }
        }

        private void Interpolate()
        {
            _targetZoomOut = _player.IsMoving && !_player.IsUnderwater ? _player.Orientation * 2.0f : Vector3.Zero;
            if (DefaultDelegate == PositionDelegate)
                _targetZoomOut += _player.IsJumping ? Vector3.UnitY * 2f : Vector3.Zero;
            _interpolatedZoomOut = Mathf.Lerp(_interpolatedZoomOut, _targetZoomOut, Time.DeltaTime * 2f);
            _interpolatedPosition = PositionDelegate() - _interpolatedZoomOut;
            Pitch = GameSettings.SmoothCamera
                ? Mathf.Lerp(Pitch, TargetPitch, Time.IndependentDeltaTime * 16f)
                : TargetPitch;
            Yaw = GameSettings.SmoothCamera ? Mathf.Lerp(Yaw, TargetYaw, Time.IndependentDeltaTime * 16f) : TargetYaw;
            Distance = Mathf.Lerp(Distance, TargetDistance + AddedDistance * Time.TimeScale,
                Time.IndependentDeltaTime * 3f);
        }

        private void ClampYaw()
        {
            if (TargetYaw > Math.PI * 2f)
            {
                TargetYaw -= (float)Math.PI * 2f;
                Yaw -= (float)Math.PI * 2f;
            }

            if (TargetYaw < -Math.PI * 2f)
            {
                TargetYaw += (float)Math.PI * 2f;
                Yaw += (float)Math.PI * 2f;
            }
        }

        public override void OnMouseWheel(object Sender, MouseWheelEventArgs E)
        {
            if (GameSettings.Paused || !CaptureMovement) return;

            var delta = E.OffsetY;
            var newDistance = TargetDistance - delta * WheelSpeed;
            if (IsColliding(GetCameraEyePosition(newDistance), out _) ||
                IsColliding(GetCameraEyePosition(newDistance + DistanceBuffer), out _)) return;

            TargetDistance -= delta * WheelSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, 1.5f, MaxDistance);
            _wheelDelta = E.OffsetY;
        }

        private Vector3 GetCameraOrientation(float CameraDistance)
        {
            return CameraDistance * LookAtPoint;
        }

        private Vector3 GetCameraOrientation()
        {
            return GetCameraOrientation(Distance);
        }

        private Vector3 GetCameraEyePosition(float CameraDistance)
        {
            return _interpolatedPosition - GetCameraOrientation(CameraDistance) + CameraHeight;
        }

        private Vector3 GetCameraEyePosition()
        {
            return _interpolatedPosition - GetCameraOrientation() + CameraHeight;
        }

        private bool IsColliding(Vector3 Position, out float NewDistance)
        {
            NewDistance = 0;
            if (AllowClipping) return false;
            BulletPhysics.ResetCallback(_callback);
            _callback.CollisionFilterMask = (int)(CollisionFilterGroups.StaticFilter | BulletPhysics.TerrainFilter);
            var src = Position.Compatible();
            var dst = _player.Position.Compatible() + BulletSharp.Math.Vector3.UnitY;
            _callback.RayFromWorld = dst;
            _callback.RayToWorld = src;
            BulletPhysics.Raycast(ref dst, ref src, _callback);
            NewDistance = (dst - _callback.HitPointWorld).Compatible().LengthFast();
            return _callback.HasHit;
        }
    }
}