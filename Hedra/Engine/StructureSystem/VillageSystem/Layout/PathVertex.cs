using Hedra.Engine.ItemSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Layout
{
    internal class PathVertex
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