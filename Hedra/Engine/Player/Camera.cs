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
    public class Camera : EventListener
    {
        public static Vector3 DefaultCameraHeight = Vector3.UnitY * 9.0f;
        public static Func<Vector3> DefaultDelegate;
        public const float DefaultMaxDistance = 10f;
        public const float DefaultMinDistance = 2.0f;
        public const float DefaultMaxPitch = 2f;
        public const float DefaultMinPitch = -2f;

        private Vector3 _cameraHeight = Vector3.UnitY * 12.0f;
        private readonly LocalPlayer _player;
        private float _prevAlpha = -1f;
        private float _prevDistance;
        private float _distance;

        public bool Check = true;
        public float TargetDistance = 10f;
        public bool LockMouse = true;
        public Vector3 LookAtPoint;
        public Matrix4 Matrix;
        public float MaxDistance = DefaultMaxDistance;
        public float MinDistance = DefaultMinDistance;
        public float MaxPitch = DefaultMaxPitch;
        public float MinPitch = DefaultMinPitch;
        public float Pitch;
        public bool PlayerMode = true;
        public Vector3 Position;
        public Func<Vector3> PositionDelegate;
        public bool ThirdPerson = true;
        public int XDelta;
        public float Yaw;
        public int YDelta;

        public float AddonDistance { get; set; }

        public Vector3 CameraHeight
        {
            get
            {
                if (_player.IsSitting || _player.IsSleeping)
                    return _cameraHeight - Vector3.UnitY * 2.0f;

                return _cameraHeight;
            }
            set { _cameraHeight = value; }
        }

        public Vector3 CameraPosition =>
            Objective - LookAtPoint * new Vector3(TargetDistance, TargetDistance, TargetDistance) + CameraHeight;

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

        public Vector3 CrossPosition
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

        public Vector3 Objective => PositionDelegate();

        public Matrix4 ViewMatrix => Matrix4.LookAt(-LookAtPoint * TargetDistance + CameraHeight,
            LookAtPoint * TargetDistance + CameraHeight,
            Vector3.UnitY);

        public Camera(LocalPlayer RefPlayer)
        {
            _player = RefPlayer;

            if (Camera.DefaultDelegate == null)
            {
                Camera.DefaultDelegate = () => _player.Model.Model.Position;
            }
            this.PositionDelegate = Camera.DefaultDelegate;
        }

        public void Update()
        {
            _distance = Mathf.Lerp(_distance, TargetDistance+AddonDistance, Time.unScaledDeltaTime * 3f);
            if ( !GameSettings.Paused && !Scenes.SceneManager.Game.IsLoading && !_player.IsDead)
            {
                XDelta = Cursor.Position.X - Constants.WIDTH / 2;
                YDelta = Cursor.Position.Y - Constants.HEIGHT / 2;

                if (LockMouse)
                    Cursor.Position =
                        new Point(Constants.WIDTH / 2, Constants.HEIGHT / 2);

                if (PlayerMode && Check)
                {
                    Yaw += XDelta * GameSettings.MouseSensibility * 0.0025f;

                    float testPitch = 0;
                    if (!GameSettings.InvertMouse)
                        testPitch -= YDelta * GameSettings.MouseSensibility * 0.0025f;
                    else
                        testPitch += YDelta * GameSettings.MouseSensibility * 0.0025f;

                    Vector3 camPos =
                        Objective - new Vector3((float) Math.Cos(Yaw), Pitch + testPitch, (float) Math.Sin(Yaw)) *
                        new Vector3(TargetDistance, TargetDistance, TargetDistance) + CameraHeight;

                    float heightAtCamPos = Physics.HeightAtPosition(camPos);
                    if (camPos.Y > heightAtCamPos + MinDistance)
                        Pitch += testPitch;

                    Pitch = Mathf.Clamp(Pitch, MinPitch, MaxPitch);
                }

                LookAtPoint = new Vector3((float) Math.Cos(Yaw), Pitch, (float) Math.Sin(Yaw));

                Vector3 pos = CameraPosition;
                float y = Physics.HeightAtPosition(pos), addonDistance = 0;
                if (pos.Y <= y + MinDistance ||
                    Physics.IsColliding(pos, new Box(-Vector3.One * 2f + pos, Vector3.One * 2f + pos)))
                {
                    if (_prevDistance == 0)
                        _prevDistance = TargetDistance;
                    TargetDistance += Time.unScaledDeltaTime * -24f;
                }
                else
                {
                    Vector3 pos2 = Objective + CameraHeight - LookAtPoint * _prevDistance;
                    float y2 = Physics.HeightAtPosition(pos2);

                    if (_prevDistance != 0 && !(pos2.Y <= y2 + MinDistance) &&
                        !Physics.IsColliding(pos2, new Box(-Vector3.One * 2f + pos2, Vector3.One * 2f + pos2)) &&
                        TargetDistance < _prevDistance)
                    {
                        Vector3 pos3 = Objective + CameraHeight -
                                       LookAtPoint * (TargetDistance + (_prevDistance - TargetDistance) * .5f);
                        float y3 = Physics.HeightAtPosition(pos3);
                        if (!(pos3.Y <= y3 + MinDistance) && !Physics.IsColliding(pos3,
                                new Box(-Vector3.One * 2f + pos3, Vector3.One * 2f + pos3)))
                        {
                            Vector3 pos4 = Objective + CameraHeight -
                                           LookAtPoint * (TargetDistance + (_prevDistance - TargetDistance) * .25f);
                            float y4 = Physics.HeightAtPosition(pos4);
                            if (!(pos4.Y <= y4 + MinDistance) && !Physics.IsColliding(pos4,
                                    new Box(-Vector3.One * 2f + pos3, Vector3.One * 2f + pos4)))
                                TargetDistance += Time.unScaledDeltaTime * 24f;
                        }
                    }
                }

                TargetDistance = Mathf.Clamp(TargetDistance, 1.5f, MaxDistance);
                if (TargetDistance < 4.5f)
                {
                    if (_prevAlpha == -1)
                        _prevAlpha = _player.Model.Alpha;
                    _player.Model.Alpha = Mathf.Clamp((TargetDistance - 1.5f) / 4.5f, 0, 1) + 0.0025f;
                }
                else
                {
                    if (_prevAlpha != -1)
                    {
                        _player.Model.Alpha = _prevAlpha;
                        _prevAlpha = -1;
                    }
                }
            }
            if (!Constants.LOCK_FRUSTUM)
                DrawManager.FrustumObject.CalculateFrustum(DrawManager.FrustumObject.ProjectionMatrix, Matrix);
        }

        public void InvertPitch()
        {
            Pitch = -Pitch;
        }

        public void InvertFacing()
        {
            Yaw = -Yaw;
        }

        //This is for rendering the images
        public void RebuildMatrix(Vector3 MatrixPosition)
        {
            LookAtPoint = new Vector3((float) Math.Cos(Yaw), Pitch, (float) Math.Sin(Yaw));
            Matrix = Matrix4.LookAt(MatrixPosition - LookAtPoint * _distance + Vector3.UnitY * 4f,
                MatrixPosition + LookAtPoint * _distance, Vector3.UnitY);
        }

        public void RebuildMatrix()
        {
            LookAtPoint = new Vector3((float) Math.Cos(Yaw), Pitch, (float) Math.Sin(Yaw));
            Matrix = Matrix4.LookAt(Objective - LookAtPoint * _distance + CameraHeight,
                Objective + LookAtPoint * _distance + CameraHeight, Vector3.UnitY);
        }


        public override void OnMouseWheel(object Sender, MouseWheelEventArgs E)
        {
            if (GameSettings.Paused || !Check) return;

            Vector3 pos = Objective - LookAtPoint * (TargetDistance - E.Delta) + CameraHeight;
            float y = Physics.HeightAtPosition(pos);
            if (pos.Y <= y + MinDistance) return;

            TargetDistance -= E.Delta;
            TargetDistance = Mathf.Clamp(TargetDistance, 1.5f, MaxDistance);
            if (TargetDistance < 4.5f)
                _player.Model.Alpha = Mathf.Clamp((TargetDistance - 1.5f) / 4.5f, 0, 1) + 0.0025f;
            else
                _player.Model.Alpha = 1;

            _prevDistance = TargetDistance;
        }
    }
}