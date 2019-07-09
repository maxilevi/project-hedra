using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Input;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.StructureSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Input;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public class Camera : EventListener, ICamera
    {
        public static Vector3 DefaultCameraHeight = Vector3.UnitY * 9.0f;
        public static Func<Vector3> DefaultDelegate;
        private const float DefaultDistance = 10;
        public const float DefaultMaxDistance = 20f;
        public const float DefaultMinDistance = 2.0f;
        public const float DefaultMaxPitch = 1.5f;
        public const float DefaultMinPitch = -2f;

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
        public Matrix4 ModelViewMatrix { get; private set; }
        public Func<Vector3> PositionDelegate { get; set; }
        private float _xDelta;
        private float _yDelta;
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float StackedYaw { get; private set; }

        private float _previousAlpha = -1f;
        private Vector3 _interpolatedPosition;
        private Vector3 _lastDelegateValue;
        private Vector3 _interpolatedZoomOut;
        private Vector3 _targetZoomOut;
        private Vector2 _lastStructureCollisionPosition;
        private Vector2 _lastChunkCollisionPosition;
        private readonly IPlayer _player;
        private readonly List<ICollidable> _structureCollisions;
        private readonly List<ICollidable> _chunkCollisions;
        private readonly Box _cameraBox;

        public Camera(IPlayer Player)
        {
            _player = Player;
            _cameraBox = new Box(-Vector3.One, Vector3.One);
            _structureCollisions = new List<ICollidable>();
            _chunkCollisions = new List<ICollidable>();
            Reset();
        }

        public void Reset()
        {
            if (DefaultDelegate == null)
            {
                DefaultDelegate = () => _player.Model.ModelPosition;
            }
            PositionDelegate = Camera.DefaultDelegate;
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

            if ( !GameSettings.Paused && !GameManager.IsLoading && !_player.IsDead && !GameSettings.ContinousMove)
            {
                _xDelta = Cursor.Position.X - GameSettings.Width / 2f;
                _yDelta = Cursor.Position.Y - GameSettings.Height / 2f;
                if (LockMouse) Cursor.Center();
                
                if (CaptureMovement)
                {
                    ManageRotations();
                }

            }
            bool ShouldMove()
            {
                var cameraPosition = CameraEyePosition;
                return IsColliding(cameraPosition) || cameraPosition.Y - 2 < Physics.HeightAtPosition(cameraPosition);
            }

            while(ShouldMove() && !GameManager.IsLoading && !GameManager.InStartMenu && TargetDistance > MinDistance)
            {
                TargetDistance -= Time.IndependentDeltaTime;
                Distance -= Time.IndependentDeltaTime;
            }

            BuildCameraMatrix();
            Culling.BuildFrustum(ModelViewMatrix);
        }

        private void ManageRotations()
        {
            TargetYaw += _xDelta * GameSettings.MouseSensibility * 0.0025f;
            StackedYaw += _xDelta * GameSettings.MouseSensibility * 0.0025f;

            var possiblePitch = (!GameSettings.InvertMouse ? -_yDelta : _yDelta) * GameSettings.MouseSensibility * 0.0025f;
            TargetPitch = Mathf.Clamp(TargetPitch + possiblePitch, MinPitch, MaxPitch);
        }

        private void Interpolate()
        {
            _targetZoomOut = _player.IsMoving && !_player.IsUnderwater ? _player.Orientation * 2.0f : Vector3.Zero;
            if (DefaultDelegate == PositionDelegate)
            {
                _targetZoomOut += _player.IsJumping ? Vector3.UnitY * 2f : Vector3.Zero;
            }
            _interpolatedZoomOut = Mathf.Lerp(_interpolatedZoomOut, _targetZoomOut, Time.DeltaTime * 2f);
            _interpolatedPosition = PositionDelegate() - _interpolatedZoomOut;
            Pitch = Mathf.Lerp(Pitch, TargetPitch, Time.IndependentDeltaTime * 16f);
            Yaw = Mathf.Lerp(Yaw, TargetYaw, Time.IndependentDeltaTime * 16f);
            Distance = Mathf.Lerp(Distance, TargetDistance + AddedDistance * Time.TimeScale, Time.IndependentDeltaTime * 3f);
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


        public void BuildCameraMatrix()
        {
            ModelViewMatrix = Matrix4.LookAt(CameraEyePosition, CameraLookAtPosition, Vector3.UnitY);
        }

        public override void OnMouseWheel(object Sender, MouseWheelEventArgs E)
        {
            if (GameSettings.Paused || !CaptureMovement) return;

            var pos = _interpolatedPosition - LookAtPoint * (TargetDistance - E.Delta * WheelSpeed) + CameraHeight;
            var y = Physics.HeightAtPosition(pos);
            if (pos.Y <= y + MinDistance) return;

            TargetDistance -= E.Delta * WheelSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, 1.5f, MaxDistance);
        }

        public float TargetDistance { get; set; } = DefaultDistance;

        private Vector3 LookAtPoint => new Vector3(Forward.X, Pitch, Forward.Z);

        public Vector3 LookingDirection => LookAtPoint.NormalizedFast();

        public Vector3 Forward => new Vector3((float)Math.Cos(Yaw), 0, (float)Math.Sin(Yaw));

        public Vector3 Backward => -Forward;

        public Vector3 Right => new Vector3((float)Math.Cos(Yaw + Math.PI / 2), 0, (float)Math.Sin(Yaw + Math.PI / 2));

        public Vector3 Left => -Right;

        public Vector3 CameraHeight { get; set; }

        public Vector3 CameraEyePosition => _interpolatedPosition - CameraOrientation + CameraHeight;

        private Vector3 CameraLookAtPosition => _interpolatedPosition + CameraOrientation + CameraHeight;
        
        private Vector3 CameraOrientation => LookAtPoint * Distance;

        public Vector3 CrossDirection
        {
            get
            {
                var lookingDir = new Vector4(0f, 0f, -1.0f, 1.0f);
                lookingDir = Vector4.Transform(lookingDir, Culling.ProjectionMatrix.Inverted());
                lookingDir = new Vector4(lookingDir.X, lookingDir.Y, -1f, 0f);
                lookingDir = Vector4.Transform(lookingDir, Culling.ModelViewMatrix.Inverted());
                return lookingDir.Xyz.NormalizedFast();
            }
        }

        public Matrix4 ViewMatrix => Matrix4.LookAt(-CameraOrientation, CameraOrientation, Vector3.UnitY);

        private bool IsColliding(Vector3 Position)
        {
            Collision.Update(
                Position,
                _chunkCollisions,
                _structureCollisions,
                ref _lastChunkCollisionPosition,
                ref _lastStructureCollisionPosition
            );
            try
            {
                _cameraBox.Translate(Position);
                for (var i = 0; i < _structureCollisions.Count; i++)
                {
                    if (Physics.Collides(_structureCollisions[i], _cameraBox))
                        return true;
                }
                for (var i = 0; i < _chunkCollisions.Count; i++)
                {
                    if (Physics.Collides(_chunkCollisions[i], _cameraBox))
                        return true;
                }
                return false;
            }
            finally
            {
                _cameraBox.Translate(-Position);
            }
        }
    }
}