using System.Numerics;

namespace Hedra.Engine.Rendering.Frustum
{
    public class Plane
    {
        public Plane() : this(Vector3.UnitZ, 1)
        {
        }

        public Plane(Vector3 Normal, float Distance)
        {
            this.Normal = Normal;
            this.Distance = Distance;
        }

        public Vector3 Normal { get; set; }
        public float Distance { get; set; }

        public float Equation(Vector3 Point)
        {
            return Vector3.Dot(Point, Normal) - Distance;
        }
    }
}