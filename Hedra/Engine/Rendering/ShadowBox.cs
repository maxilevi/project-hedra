//ShadowBox class ported from ThinMatrix code

using System;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class ShadowBox
    {
        private const float Offset = 10;
        private const float ShadowDistance = 100;
        private static readonly Vector3 Up = new Vector3(0, 1, 0);
        private static readonly Vector3 Forward = new Vector3(0, 0, -1);

        public Matrix4 LightMatrix;
        public Matrix4 Matrix { get; private set; }


        private readonly Camera _view;
        private float _minX, _maxX;
        private float _minY, _maxY;
        private float _minZ, _maxZ;
        private float _farHeight;
        private float _farWidth;
        private float _nearHeight;
        private float _nearWidth;

        public ShadowBox(Matrix4 LightMatrix, Camera View)
        {
            this.LightMatrix = LightMatrix;
            this._view = View;
            this.CalculateWidthsAndHeights();
        }

        public Matrix4 Update()
        {
            Matrix4 rotation = this.CameraRotationMatrix;
            Vector3 forwardVector = new Vector3(Vector3.TransformNormal(Forward, rotation));

            Vector3 toFar = forwardVector * ShadowDistance;
            Vector3 toNear = forwardVector * FrustumCulling.ZNear;
            Vector3 centerNear = toNear + _view.CameraPosition;
            Vector3 centerFar = toFar + _view.CameraPosition;

            Vector4[] points = this.CalculateFrustumVertices(rotation, forwardVector, centerNear, centerFar);

            bool first = true;
            for (int i = 0; i < points.Length; i++)
            {
                if (first)
                {
                    _minX = points[i].X;
                    _maxX = points[i].X;
                    _minY = points[i].Y;
                    _maxY = points[i].Y;
                    _minZ = points[i].Z;
                    _maxZ = points[i].Z;
                    first = false;
                    continue;
                }
                if (points[i].X > _maxX)
                {
                    _maxX = points[i].X;
                }
                else if (points[i].X < _minX)
                {
                    _minX = points[i].X;
                }
                if (points[i].Y > _maxY)
                {
                    _maxY = points[i].Y;
                }
                else if (points[i].Y < _minY)
                {
                    _minY = points[i].Y;
                }
                if (points[i].Z > _maxZ)
                {
                    _maxZ = points[i].Z;
                }
                else if (points[i].Z < _minZ)
                {
                    _minZ = points[i].Z;
                }
            }
            _maxZ += Offset;

            Matrix = Matrix4.CreateOrthographicOffCenter(_minX, _maxX, _minY, _maxY, _minZ, _maxZ);
            return Matrix;
        }

        public float Width => _maxX - _minX;

        public float Height => _maxY - _minY;

        public float Length => _maxZ - _minZ;

        private Matrix4 CameraRotationMatrix => new Matrix4(new Matrix3(_view.ModelViewMatrix));


        private Vector4[] CalculateFrustumVertices(Matrix4 Rotation, Vector3 ForwardVector, Vector3 CenterNear, Vector3 CenterFar)
        {
            Vector3 upVector = new Vector3(Vector3.TransformNormal(Up, Rotation));
            Vector3 rightVector = Vector3.Cross(ForwardVector, upVector);
            Vector3 downVector = new Vector3(-upVector.X, -upVector.Y, -upVector.Z);
            Vector3 leftVector = new Vector3(-rightVector.X, -rightVector.Y, -rightVector.Z);

            Vector3 farTop =
                CenterFar + new Vector3(upVector.X * _farHeight, upVector.Y * _farHeight, upVector.Z * _farHeight);
            Vector3 farBottom = CenterFar + new Vector3(downVector.X * _farHeight, downVector.Y * _farHeight,
                                    downVector.Z * _farHeight);
            Vector3 nearTop = CenterNear +
                              new Vector3(upVector.X * _nearHeight, upVector.Y * _nearHeight, upVector.Z * _nearHeight);
            Vector3 nearBottom = CenterNear + new Vector3(downVector.X * _nearHeight, downVector.Y * _nearHeight,
                                     downVector.Z * _nearHeight);

            Vector4[] points = new Vector4[8];
            points[0] = this.CalculateLightSpaceFrustumCorner(farTop, rightVector, _farWidth);
            points[1] = this.CalculateLightSpaceFrustumCorner(farTop, leftVector, _farWidth);
            points[2] = this.CalculateLightSpaceFrustumCorner(farBottom, rightVector, _farWidth);
            points[3] = this.CalculateLightSpaceFrustumCorner(farBottom, leftVector, _farWidth);
            points[4] = this.CalculateLightSpaceFrustumCorner(nearTop, rightVector, _nearWidth);
            points[5] = this.CalculateLightSpaceFrustumCorner(nearTop, leftVector, _nearWidth);
            points[6] = this.CalculateLightSpaceFrustumCorner(nearBottom, rightVector, _nearWidth);
            points[7] = this.CalculateLightSpaceFrustumCorner(nearBottom, leftVector, _nearWidth);
            return points;
        }

        private Vector4 CalculateLightSpaceFrustumCorner(Vector3 StartPoint, Vector3 Direction, float Width)
        {
            Vector3 point = StartPoint + new Vector3(Direction.X * Width, Direction.Y * Width, Direction.Z * Width);
            Vector4 point4F = new Vector4(point.X, point.Y, point.Z, 1f);
            return Vector4.Transform(LightMatrix, point4F);
        }

        private void CalculateWidthsAndHeights()
        {
            _farWidth = (float) (ShadowDistance * Math.Tan(Mathf.Radian * GameSettings.Fov));
            _nearWidth = (float) (FrustumCulling.ZNear * Math.Tan(Mathf.Radian * GameSettings.Fov));
            _farHeight = _farWidth / FrustumCulling.Aspect;
            _nearHeight = _nearWidth / FrustumCulling.Aspect;
        }
    }
}