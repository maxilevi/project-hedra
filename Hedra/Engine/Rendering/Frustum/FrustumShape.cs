using OpenTK;

namespace Hedra.Engine.Rendering.Frustum
{
    public class FrustumShape
    {
        private const int CornerCount = 8;
        private const int PlaneCount = 6;
        private readonly Vector3[] _corners = new Vector3[CornerCount];
        private readonly Plane[] _planes = new Plane[PlaneCount];
        private Matrix4 _matrix;

        public Vector3[] Corners => _corners;
        
        public Matrix4 Matrix
        {
            get => _matrix;
            set
            {
                _matrix = value;
                CreatePlanes();    // FIXME: The odds are the planes will be used a lot more often than the matrix
                CreateCorners();   // is updated, so this should help performance. I hope ;)
            }
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