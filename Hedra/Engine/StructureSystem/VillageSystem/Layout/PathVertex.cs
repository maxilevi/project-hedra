using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Layout
{
    internal class PathVertex
    {
        public Vector3 Point { get; set; }

        public float X => Point.X;
        public float Y => Point.Y;
        public float Z => Point.Z;
    }
}