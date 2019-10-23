using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapBaseItem
    {
        public ObjectMesh Mesh { get; set; }

        public MapBaseItem(int MapSize)
        {
            HasChunk = new bool[MapSize * MapSize];
        }

        public MapBaseItem(int MapSize, ObjectMesh Mesh) : this(MapSize)
        {
            this.Mesh = Mesh;
        }

        public Vector2 Coordinates { get; set; }
        public bool[] HasChunk { get; set; }
        public bool WasBuilt { get; set; }

        public static bool UsableChunk(Chunk Chunk)
        {
            return Chunk != null && Chunk.BuildedWithStructures;
        }
    }
}
