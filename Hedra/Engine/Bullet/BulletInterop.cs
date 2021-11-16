using System.Numerics;
using Vector4 = BulletSharp.Math.Vector4;

namespace Hedra.Engine.Bullet
{
    public static class BulletInterop
    {
        public static Vector3 Compatible(this BulletSharp.Math.Vector3 Vector)
        {
            return new Vector3(Vector.X, Vector.Y, Vector.Z);
        }

        public static BulletSharp.Math.Vector3 Compatible(this Vector3 Vector)
        {
            return new BulletSharp.Math.Vector3(Vector.X, Vector.Y, Vector.Z);
        }

        public static Vector4 Compatible(this System.Numerics.Vector4 Vector)
        {
            return new Vector4(Vector.X, Vector.Y, Vector.Z, Vector.W);
        }
    }
}