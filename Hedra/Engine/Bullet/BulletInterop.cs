using BulletSharp.Math;
using OpenTK;
using Vector3 = BulletSharp.Math.Vector3;

namespace Hedra.Engine.Bullet
{
    public static class BulletInterop
    {
        public static OpenTK.Vector3 Compatible(this Vector3 Vector)
        {
            return new OpenTK.Vector3(Vector.X, Vector.Y, Vector.Z);
        }
        
        public static Vector3 Compatible(this OpenTK.Vector3 Vector)
        {
            return new Vector3(Vector.X, Vector.Y, Vector.Z);
        }
        
        public static Matrix Compatible(this Matrix4 Matrix)
        {
            var matrix = new Matrix
            {
                M11 = Matrix.M11,
                M12 = Matrix.M12,
                M13 = Matrix.M13,
                M14 = Matrix.M14,
                M21 = Matrix.M21,
                M22 = Matrix.M22,
                M23 = Matrix.M23,
                M24 = Matrix.M24,
                M31 = Matrix.M31,
                M32 = Matrix.M32,
                M33 = Matrix.M33,
                M34 = Matrix.M34,
                M41 = Matrix.M41,
                M42 = Matrix.M42,
                M43 = Matrix.M43,
                M44 = Matrix.M44
            };
            return matrix;
        }
    }
}