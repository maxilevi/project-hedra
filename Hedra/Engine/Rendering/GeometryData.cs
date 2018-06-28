using OpenTK;

namespace Hedra.Engine.Rendering
{
    internal class GeometryData
    {
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector2[] UVs { get; set; }
        public ushort[] Indices { get; set; }
    }
}
