using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class WaterEdgePatcher
    {
        public VertexData Process(VertexData Water, int Lod)
        {
            if (Lod != 1) return Water;

            var indexedVertices = new Dictionary<Vector3, List<int>> ();
            for (var i = 0; i < Water.Vertices.Count; i++)
            {
                var key = Water.Vertices[i];
                if (!indexedVertices.ContainsKey(key)) indexedVertices.Add(key, new List<int>());
                indexedVertices[key].Add(i);
            }

            var borders = DetectBorders(Water);
            for (var i = 0; i < borders.Length; i++)
            {
                var indices = indexedVertices[borders[i].Vertex];
                for (var k = 0; k < indices.Count; k++)
                {
                    var norm = (int) borders[i].Vertex.X == 0 || (int) borders[i].Vertex.Z == 0 || (int)borders[i].Vertex.X == Chunk.Width || (int)borders[i].Vertex.Z == Chunk.Width
                        ? Vector3.Zero : borders[i].Direction.ToVector3();
                    Water.Vertices[indices[k]] += norm * Lod; 
                }
            }
            return Water;
        }

        /// <summary>
        /// Calculate all the edges of the water mesh.
        /// </summary>
        /// <param name="Water">A VertexData object representing the water mesh.</param>
        /// <returns>Returns an array of vertices that have no neighbouring vertices.</returns>
        private BorderVertex[] DetectBorders(VertexData Water)
        {
            var indexedVertices = new Dictionary<Vector2, List<Vector3>>();
            for (var i = 0; i < Water.Vertices.Count; i++)
            {
                var key = Water.Vertices[i].Xz;
                if (!indexedVertices.ContainsKey(key)) indexedVertices.Add(key, new List<Vector3>());
                indexedVertices[key].Add(Water.Vertices[i]);
            }
            var edges = new List<BorderVertex>();
            foreach (var pair in indexedVertices)
            {
                if (IsValid(pair.Value, indexedVertices))
                {
                    var dir = CalculateDirection(pair.Value.First(), pair.Value.Count, indexedVertices);
                    for (var k = 0; k < pair.Value.Count; k++)
                    {
                        edges.Add(new BorderVertex
                        {
                            Vertex = pair.Value[k],
                            Direction = dir
                        });
                    }
                }
            }
            return edges.ToArray();
        }

        private Vector2 CalculateDirection(Vector3 Position, int ConnectionCount, Dictionary<Vector2, List<Vector3>> IndexedList)
        {
            var pos = Position.Xz;
            var accumDir = Vector2.Zero;
            var dir = Vector2.Zero;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(Chunk.BlockSize, 0))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(0, -Chunk.BlockSize))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(0, Chunk.BlockSize))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(-Chunk.BlockSize, 0))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(Chunk.BlockSize, Chunk.BlockSize))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(-Chunk.BlockSize, Chunk.BlockSize))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(Chunk.BlockSize, -Chunk.BlockSize))) ? Vector2.Zero : dir;
            accumDir += IndexedList.ContainsKey(pos + (dir = new Vector2(-Chunk.BlockSize, -Chunk.BlockSize))) ? Vector2.Zero : dir;
            return accumDir == Vector2.Zero ? Vector2.Zero : accumDir.NormalizedFast() * Chunk.BlockSize;
        }

        private bool IsValid(List<Vector3> Connections, Dictionary<Vector2, List<Vector3>> IndexedList)
        {
            return Connections.Count == 2 || Connections.Count == 3 || Connections.Count == 6 ||
                Connections.Count == 4 && !IsInvalid4Connections(Connections.First(), IndexedList);
        }

        private bool IsInvalid4Connections(Vector3 Position, Dictionary<Vector2, List<Vector3>> IndexedList)
        {
            return IndexedList.ContainsKey(new Vector2(Position.X + Chunk.BlockSize, Position.Z)) &&
                   IndexedList.ContainsKey(new Vector2(Position.X - Chunk.BlockSize, Position.Z)) &&
                   IndexedList.ContainsKey(new Vector2(Position.X, Position.Z + Chunk.BlockSize)) &&
                   IndexedList.ContainsKey(new Vector2(Position.X, Position.Z - Chunk.BlockSize));
        }

        struct BorderVertex
        {
            public Vector3 Vertex { get; set; }
            public Vector2 Direction { get; set; }
        } 
    }
}
