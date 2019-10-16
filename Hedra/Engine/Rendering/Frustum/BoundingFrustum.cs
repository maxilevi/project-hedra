using System;
using System.Linq;
using BulletSharp;
using Hedra.Core;
using Hedra.Game;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Frustum
{
    public class BoundingFrustum
    {
        private static readonly Vector4[] UnitCube = new Vector4[8]
        {
            new Vector4(-1, -1, -1, 1),
            new Vector4(-1, -1, 1, 1),
            new Vector4(-1, 1, -1, 1),
            new Vector4(1, -1, -1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(1, 1, -1, 1),
            new Vector4(1, 1, 1, 1),
        };
        
        private Matrix4x4 _projection;
        private Matrix4x4 _modelView;
        private readonly Vector3[] _corners;
        private readonly Plane[] _planes;
        private readonly Vector3[] _cubeCachePoints;

        public BoundingFrustum()
        {
            _cubeCachePoints = new Vector3[8];
            _corners = new Vector3[8];
            _planes = new[]
            {
                new Plane(),
                new Plane(),
                new Plane(),
                new Plane(),
                new Plane(),
                new Plane(),
            };
        }
        
        public bool Contains(ref Vector3 Min, ref  Vector3 Max)
        {
            _cubeCachePoints[0] = Min;
            _cubeCachePoints[1] = new Vector3(Max.X, Min.Y, Max.Z);
            _cubeCachePoints[2] = new Vector3(Max.X, Min.Y, Min.Z);
            _cubeCachePoints[3] = new Vector3(Min.X, Min.Y, Max.Z);
            _cubeCachePoints[4] = new Vector3(Min.X, Max.Y, Min.Z);
            _cubeCachePoints[5] = new Vector3(Min.X, Max.Y, Max.Z);
            _cubeCachePoints[6] = new Vector3(Max.X, Max.Y, Min.Z);
            _cubeCachePoints[7] = Max;
            return FrustumGJK.Collides(_corners, _cubeCachePoints);
        }
        
        public void SetMatrices(Matrix4x4 Projection, Matrix4x4 ModelView)
        {
            _projection = Projection;
            _modelView = ModelView;
            CreateCorners();
            CreatePlanes();
        }

        private void CreatePlanes()
        {
            var view = _modelView.Transposed();
            var viewProjection = _projection * view;

            LeftPlane.Normal = (viewProjection.Column3() + viewProjection.Column0()).Xyz();
            RightPlane.Normal = (viewProjection.Column3() - viewProjection.Column0()).Xyz();
            BottomPlane.Normal = (viewProjection.Column3() + viewProjection.Column1()).Xyz();
            TopPlane.Normal = (viewProjection.Column3() - viewProjection.Column1()).Xyz();
            NearPlane.Normal = (viewProjection.Column2()).Xyz();
            FarPlane.Normal = (viewProjection.Column3() - viewProjection.Column2()).Xyz();

            LeftPlane.Distance = viewProjection.M44 + viewProjection.M41;
            RightPlane.Distance = viewProjection.M44 - viewProjection.M41;
            BottomPlane.Distance = viewProjection.M44 + viewProjection.M42;
            TopPlane.Distance = viewProjection.M44 - viewProjection.M42;
            NearPlane.Distance = -viewProjection.M43;
            FarPlane.Distance = viewProjection.M44 - viewProjection.M43;

            for (var i = 0; i < _planes.Length; ++i)
            {
                var invMagnitude = 1f / _planes[i].Normal.Length();
                _planes[i].Normal *= invMagnitude;
                _planes[i].Distance *= invMagnitude;
            }
        }

        private bool Intersects(ref Vector3 Min, ref Vector3 Max)
        {
            for (var i = 0; i < _planes.Length; ++i)
            {
                var output = 0;
                for (var k = 0; k < _cubeCachePoints.Length; ++k)
                    output += (Vector3.Dot(_planes[i].Normal, _cubeCachePoints[k]) < 0 ? 1 : 0);
                if(output == 8) return false;
            }

            return true;
        }

        private float Classify(ref Vector3 Min, ref Vector3 Max, Plane Plane)
        {
            var size = (Max - Min) * .5f;
            var r = Math.Abs(size.X * Plane.Normal.X) + Math.Abs(size.Y * Plane.Normal.Y) + Math.Abs(size.Z * Plane.Normal.Z);
            var d = Vector3.Dot(Plane.Normal, Min + size) + Plane.Distance;
            if (Math.Abs(d) < r) return 0f;
            if (d < 0f)
            {
                return d + r;
            }
            return d - r;
        }

        private void CreateCorners()
        {
            
            var invProjection = _projection.Inverted();
            var invModelView = _modelView.Inverted();
            for (var i = 0; i < _corners.Length; ++i)
            {
                var transformed = Vector4.Transform(UnitCube[i], invProjection);
                _corners[i] = Vector3.Transform((transformed.Xyz() / transformed.W), invModelView);
            }
        }

        public void Draw(Matrix4x4 DrawingMatrix)
        { 
            if (GameSettings.DebugFrustum)
            {
                var invProjection = (Matrix4x4.CreatePerspectiveFieldOfView(50 * Mathf.Radian, 1.33f, 4, 64)).Inverted();
                var position = Vector3.Zero;
                var newCorners = UnitCube.Select(V => Vector4.Transform(V, invProjection)).Select(P => (P.Xyz() / P.W) + position).Select(P => Vector3.Transform(P, DrawingMatrix)).ToArray();
                for (var i = 0; i < newCorners.Length; ++i)
                {
                    BasicGeometry.DrawPoint(newCorners[i], Vector4.One, 10);
                }

                for (var i = 0; i < _planes.Length; ++i)
                {
                    BasicGeometry.DrawLine(DrawingMatrix.ExtractTranslation() + _planes[i].Normal * 32, DrawingMatrix.ExtractTranslation() + _planes[i].Normal * 48, Vector4.One, 5);
                }
            }
        }

        private Plane LeftPlane => _planes[0];
        private Plane RightPlane => _planes[1];
        private Plane BottomPlane => _planes[2];
        private Plane TopPlane => _planes[3];
        private Plane NearPlane => _planes[4];
        private Plane FarPlane => _planes[5];
    }
}
