using System;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class MeshStitcher
    {
        private readonly ChunkMeshBorderDetector _borderDetector;
        private readonly Chunk _parent;
        
        public MeshStitcher(Chunk Parent)
        {
            _parent = Parent;
            _borderDetector = new ChunkMeshBorderDetector();
        }
        
        public VertexData Process(VertexData Terrain, Vector3 Size)
        {
            var borders = _borderDetector.Process(Terrain, Size);
            ProcessBorder(Terrain, borders.FrontBorder,
                World.GetChunkAt(_parent.Position + new Vector3(0, 0, Chunk.Width)), 
                (X, Y, Z) => Z == 0
            );
            
            ProcessBorder(Terrain, borders.RightBorder,
                World.GetChunkAt(_parent.Position + new Vector3(Chunk.Width, 0, 0)),
                (X, Y, Z) => X == 0
            );

            Chunk backChunk;
            ProcessBorder(Terrain, borders.BackBorder,
                backChunk = World.GetChunkAt(_parent.Position + new Vector3(0, 0, -Chunk.Width)),
                (X, Y, Z) => Z == backChunk.BoundsZ - backChunk.Lod
            );

            Chunk leftChunk;
            ProcessBorder(Terrain, borders.LeftBorder,
                leftChunk = World.GetChunkAt(_parent.Position + new Vector3(-Chunk.Width, 0, 0)),
                (X, Y, Z) => X == leftChunk.BoundsX - leftChunk.Lod 
            );
            return Terrain;
        }

        private void ProcessBorder(VertexData Terrain, Vector3[] Border, Chunk Neighbour, Func<int,int,int,bool> Filter)
        {
            if(Neighbour == null || Neighbour.Lod <= _parent.Lod) return;
            var neighbourBorder = Neighbour.CreateTerrainVertices(Filter);
            for (var i = 0; i < Border.Length; i++)
            {
                Terrain.Vertices[Terrain.Vertices.IndexOf(Border[i])] = 
                    FindNearest(neighbourBorder, Border[i], Neighbour.Position - _parent.Position);
            }
        }

        /// <summary>
        /// Finds the nearest vertex in a rray, in order to stitch them.
        /// </summary>
        /// <param name="Border">Array of vertices to look into.</param>
        /// <param name="Vertex">Search candidate</param>
        /// <param name="Offset">Offset position of the array.</param>
        /// <returns>Return the nearest vertex or Vector3.Zero if none found.</returns>
        private static Vector3 FindNearest(Vector3[] Border, Vector3 Vertex, Vector3 Offset)
        {
            var dist = float.MaxValue;
            var point = Vector3.Zero;
            for (var i = 0; i < Border.Length; i++)
            {
                var borderPoint = Border[i] + Offset;
                var newDist = (borderPoint - Vertex).LengthSquared;
                if (newDist < dist)
                {
                    dist = newDist;
                    point = borderPoint;
                }
            }
            return point;
        }
    }
}