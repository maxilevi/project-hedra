using OpenTK;

namespace Hedra.Engine.Rendering.Isosurface
{
    public struct Matrix3X3
    {
        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;

        public Matrix3X3(Vector3 col1, Vector3 col2, Vector3 col3)
        {
            M11 = col1.X;
            M12 = col2.X;
            M13 = col3.X;
            M21 = col1.Y;
            M22 = col2.Y;
            M23 = col3.Y;
            M31 = col1.Z;
            M32 = col2.Z;
            M33 = col3.Z;
        }

        public Matrix3X3(float m11,float m12,float m13,
                         float m21,float m22,float m23,
                         float m31,float m32,float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public Vector3 Col1
        {
            get { return new Vector3(M11, M21, M31); }
            set { M11 = value.X; M21 = value.Y; M31 = value.Z; }
        }
        public Vector3 Col2
        {
            get { return new Vector3(M12, M22, M32); }
            set { M12 = value.X; M22 = value.Y; M32 = value.Z; }
        }
        public Vector3 Col3
        {
            get { return new Vector3(M13, M23, M33); }
            set { M13 = value.X; M23 = value.Y; M33 = value.Z; }
        }

        public static Vector3 operator *(Matrix3X3 m, Vector3 v)
        {
            return new Vector3(
                m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z,
                m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z,
                m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z);
        }

        public static Vector3i operator *(Matrix3X3 m, Vector3i v)
        {
            return new Vector3i(
                (int)(m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z),
                (int)(m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z),
                (int)(m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z));
        }
    }
}