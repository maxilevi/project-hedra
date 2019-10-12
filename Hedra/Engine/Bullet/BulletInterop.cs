using BulletSharp.Math;
using Vector3 = BulletSharp.Math.Vector3;

namespace Hedra.Engine.Bullet
{
    public static class BulletInterop
    {
        public static OpenToolkit.Mathematics.Vector3 Compatible(this Vector3 Vector)
        {
            return new OpenToolkit.Mathematics.Vector3(Vector.X, Vector.Y, Vector.Z);
        }
        
        public static Vector3 Compatible(this OpenToolkit.Mathematics.Vector3 Vector)
        {
            return new Vector3(Vector.X, Vector.Y, Vector.Z);
        }
        
        public static Vector4 Compatible(this OpenToolkit.Mathematics.Vector4 Vector)
        {
            return new Vector4(Vector.X, Vector.Y, Vector.Z, Vector.W);
        }
    }
}