// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.Rendering.Frustum
{
    /// <summary>
    /// Defines a viewing frustum for intersection operations.
    /// </summary>
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
        
        private Matrix4 _projection;
        private Matrix4 _modelView;
        private readonly Vector3[] _corners = new Vector3[8];
        private readonly Vector3[] _cubeCachePoints = new Vector3[8];

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

        public void SetMatrices(Matrix4 Projection, Matrix4 ModelView)
        {
            _projection = Projection;
            _modelView = ModelView;
            CreateCorners();
        }

        private void CreateCorners()
        {
            var invProjection = _projection.Inverted();
            var invModelView = _modelView.Inverted();
            for (var i = 0; i < _corners.Length; ++i)
            {
                var transformed = Vector4.Transform(UnitCube[i], invProjection);
                _corners[i] = Vector3.TransformPosition((transformed.Xyz / transformed.W), invModelView);
            }
        }
    }
}
