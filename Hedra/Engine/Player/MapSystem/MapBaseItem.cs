using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapBaseItem
    {
        public ObjectMesh Mesh { get; set; }

        public MapBaseItem() { }
        public MapBaseItem(ObjectMesh Mesh)
        {
            this.Mesh = Mesh;
        }

        public Vector2 Coordinates { get; set; }
        public bool HasChunk { get; set; }
        public bool WasBuilt { get; set; }
    }
}
