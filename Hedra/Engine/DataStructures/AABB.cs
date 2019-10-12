using OpenToolkit.Mathematics;

namespace Hedra.Engine.DataStructures
{
    public struct AABB
    {
        public Vector3 Origin { get; set; }
        public Vector3 Size { get; set; }

        public bool Intersects(AABB Other)
        {
            return false;
        }
    }
}