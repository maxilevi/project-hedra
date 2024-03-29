using System.Numerics;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.StructureSystem.VillageSystem.Layout
{
    public class PathVertex
    {
        public PathVertex()
        {
            Attributes = new AttributeArray();
        }

        public AttributeArray Attributes { get; }
        public Vector3 Point { get; set; }

        public float X => Point.X;
        public float Y => Point.Y;
        public float Z => Point.Z;
    }
}