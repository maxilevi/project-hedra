// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
        private Matrix4 _matrix;
        private readonly Vector3[] _corners = new Vector3[CornerCount];
        private readonly Plane[] _planes = new Plane[PlaneCount];

        /// <summary>
        /// The number of planes in the frustum.
        /// </summary>
        public const int PlaneCount = 6;

        /// <summary>
        /// The number of corner points in the frustum.
        /// </summary>
        public const int CornerCount = 8;

        
        public Vector3[] Corners => _corners;

        public bool Contains(ref Vector3 Min, ref  Vector3 Max)
        {
            var intersects = false;
            for (var i = 0; i < PlaneCount; ++i)
            {
                var planeIntersectionType = Intersects(ref Min, ref Max, ref _planes[i]);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        return false;
                    case PlaneIntersectionType.Intersecting:
                        return true;
                }
            }
            return true;
        }
        
        private PlaneIntersectionType Intersects(ref Vector3 Min, ref  Vector3 Max, ref Plane Plane)
        {
            Vector3 positiveVertex;
            Vector3 negativeVertex;

            if (Plane.Normal.X >= 0)
            {
                positiveVertex.X = Max.X;
                negativeVertex.X = Min.X;
            }
            else
            {
                positiveVertex.X = Min.X;
                negativeVertex.X = Max.X;
            }

            if (Plane.Normal.Y >= 0)
            {
                positiveVertex.Y = Max.Y;
                negativeVertex.Y = Min.Y;
            }
            else
            {
                positiveVertex.Y = Min.Y;
                negativeVertex.Y = Max.Y;
            }

            if (Plane.Normal.Z >= 0)
            {
                positiveVertex.Z = Max.Z;
                negativeVertex.Z = Min.Z;
            }
            else
            {
                positiveVertex.Z = Min.Z;
                negativeVertex.Z = Max.Z;
            }

            // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
            var distance = Plane.Normal.X * negativeVertex.X + Plane.Normal.Y * negativeVertex.Y + Plane.Normal.Z * negativeVertex.Z + Plane.D;
            if (distance > 0)
            {
                return PlaneIntersectionType.Front;
            }

            // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
            distance = Plane.Normal.X * positiveVertex.X + Plane.Normal.Y * positiveVertex.Y + Plane.Normal.Z * positiveVertex.Z + Plane.D;
            if (distance < 0)
            {
                return PlaneIntersectionType.Back;
            }

            return PlaneIntersectionType.Intersecting;
        }
        
        public bool Contains(ref Vector3 Point)
        {
            for (var i = 0; i < PlaneCount; ++i)
            {
                if (PlaneHelper.ClassifyPoint(ref Point, ref _planes[i]) > 0)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Gets or sets the <see cref="Matrix"/> of the frustum.
        /// </summary>
        public Matrix4 Matrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
                CreatePlanes();    // FIXME: The odds are the planes will be used a lot more often than the matrix
                CreateCorners();   // is updated, so this should help performance. I hope ;)
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="BoundingFrustum"/> in the format:
        /// {Near:[nearPlane] Far:[farPlane] Left:[leftPlane] Right:[rightPlane] Top:[topPlane] Bottom:[bottomPlane]}
        /// </summary>
        /// <returns><see cref="String"/> representation of this <see cref="BoundingFrustum"/>.</returns>
        public override string ToString()
        {
            return "{Near: " + _planes[0] +
                   " Far:" + _planes[1] +
                   " Left:" + _planes[2] +
                   " Right:" + _planes[3] +
                   " Top:" + _planes[4] +
                   " Bottom:" + _planes[5] +
                   "}";
        }

        private void CreateCorners()
        {
            IntersectionPoint(ref _planes[0], ref _planes[2], ref _planes[4], out _corners[0]);
            IntersectionPoint(ref _planes[0], ref _planes[3], ref _planes[4], out _corners[1]);
            IntersectionPoint(ref _planes[0], ref _planes[3], ref _planes[5], out _corners[2]);
            IntersectionPoint(ref _planes[0], ref _planes[2], ref _planes[5], out _corners[3]);
            IntersectionPoint(ref _planes[1], ref _planes[2], ref _planes[4], out _corners[4]);
            IntersectionPoint(ref _planes[1], ref _planes[3], ref _planes[4], out _corners[5]);
            IntersectionPoint(ref _planes[1], ref _planes[3], ref _planes[5], out _corners[6]);
            IntersectionPoint(ref _planes[1], ref _planes[2], ref _planes[5], out _corners[7]);
        }

        private void CreatePlanes()
        {            
            _planes[0] = new Plane(-_matrix.M13, -_matrix.M23, -_matrix.M33, -_matrix.M43);
            _planes[1] = new Plane(_matrix.M13 - _matrix.M14, _matrix.M23 - _matrix.M24, _matrix.M33 - _matrix.M34, _matrix.M43 - _matrix.M44);
            _planes[2] = new Plane(-_matrix.M14 - _matrix.M11, -_matrix.M24 - _matrix.M21, -_matrix.M34 - _matrix.M31, -_matrix.M44 - _matrix.M41);
            _planes[3] = new Plane(_matrix.M11 - _matrix.M14, _matrix.M21 - _matrix.M24, _matrix.M31 - _matrix.M34, _matrix.M41 - _matrix.M44);
            _planes[4] = new Plane(_matrix.M12 - _matrix.M14, _matrix.M22 - _matrix.M24, _matrix.M32 - _matrix.M34, _matrix.M42 - _matrix.M44);
            _planes[5] = new Plane(-_matrix.M14 - _matrix.M12, -_matrix.M24 - _matrix.M22, -_matrix.M34 - _matrix.M32, -_matrix.M44 - _matrix.M42);
            
            NormalizePlane(ref _planes[0]);
            NormalizePlane(ref _planes[1]);
            NormalizePlane(ref _planes[2]);
            NormalizePlane(ref _planes[3]);
            NormalizePlane(ref _planes[4]);
            NormalizePlane(ref _planes[5]);
        }

        private static void IntersectionPoint(ref Plane a, ref Plane b, ref Plane c, out Vector3 result)
        {
            // Formula used
            //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
            //P =   -------------------------------------------------------------------------
            //                             N1 . ( N2 * N3 )
            //
            // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product
            
            Vector3 v1, v2, v3;
            Vector3 cross;
            
            Vector3.Cross(ref b.Normal, ref c.Normal, out cross);
            
            float f;
            Vector3.Dot(ref a.Normal, ref cross, out f);
            f *= -1.0f;
            
            Vector3.Cross(ref b.Normal, ref c.Normal, out cross);
            Vector3.Multiply(ref cross, a.D, out v1);
            //v1 = (a.D * (Vector3.Cross(b.Normal, c.Normal)));
            
            
            Vector3.Cross(ref c.Normal, ref a.Normal, out cross);
            Vector3.Multiply(ref cross, b.D, out v2);
            //v2 = (b.D * (Vector3.Cross(c.Normal, a.Normal)));
            
            
            Vector3.Cross(ref a.Normal, ref b.Normal, out cross);
            Vector3.Multiply(ref cross, c.D, out v3);
            //v3 = (c.D * (Vector3.Cross(a.Normal, b.Normal)));
            
            result.X = (v1.X + v2.X + v3.X) / f;
            result.Y = (v1.Y + v2.Y + v3.Y) / f;
            result.Z = (v1.Z + v2.Z + v3.Z) / f;
        }
        
        private void NormalizePlane(ref Plane p)
        {
            float factor = 1f / p.Normal.Length;
            p.Normal.X *= factor;
            p.Normal.Y *= factor;
            p.Normal.Z *= factor;
            p.D *= factor;
        }
    }
}
