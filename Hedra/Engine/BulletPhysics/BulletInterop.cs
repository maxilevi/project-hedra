using Vector3 = BulletSharp.Vector3;

namespace Hedra.Engine.BulletPhysics
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
    }
}