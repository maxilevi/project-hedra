using BulletSharp.Math;
using Vector3 = BulletSharp.Math.Vector3;

namespace Hedra.Engine.Bullet
{
    public static class BulletInterop
    {
        public static System.Numerics.Vector3 Compatible(this Vector3 Vector)
        {
            return new System.Numerics.Vector3(Vector.X, Vector.Y, Vector.Z);
        }
        
        public static Vector3 Compatible(this System.Numerics.Vector3 Vector)
        {
            return new Vector3(Vector.X, Vector.Y, Vector.Z);
        }
        
        public static Vector4 Compatible(this System.Numerics.Vector4 Vector)
        {
            return new Vector4(Vector.X, Vector.Y, Vector.Z, Vector.W);
        }
    }
}