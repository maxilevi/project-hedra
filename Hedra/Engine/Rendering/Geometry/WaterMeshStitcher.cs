using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class WaterMeshStitcher
    {
        private readonly Chunk _parent;
        private readonly ChunkMeshBorderDetector _borderDetector;

        public WaterMeshStitcher(Chunk Parent)
        {
            _parent = Parent;
            _borderDetector = new ChunkMeshBorderDetector();
        }

        public VertexData Process(VertexData Mesh, Vector3 Size)
        {
            var borders = _borderDetector.Process(Mesh, Size);
            ProcessBorder(Mesh, borders.FrontBorder,
                World.GetChunkAt(_parent.Position + new Vector3(0, 0, Chunk.Width))
            );
            
            ProcessBorder(Mesh, borders.RightBorder,
                World.GetChunkAt(_parent.Position + new Vector3(Chunk.Width, 0, 0))
            );

            ProcessBorder(Mesh, borders.BackBorder,
                World.GetChunkAt(_parent.Position + new Vector3(0, 0, -Chunk.Width))
            );

            ProcessBorder(Mesh, borders.LeftBorder,
                World.GetChunkAt(_parent.Position + new Vector3(-Chunk.Width, 0, 0))
            );

            /* Corners */

            ProcessBorder(Mesh, borders.BackLeftCorner,
                World.GetChunkAt(_parent.Position + new Vector3(-Chunk.Width, 0, -Chunk.Width))
            );

            ProcessBorder(Mesh, borders.FrontLeftCorner,
                World.GetChunkAt(_parent.Position + new Vector3(-Chunk.Width, 0, Chunk.Width))
            );

            ProcessBorder(Mesh, borders.FrontRightCorner,
                World.GetChunkAt(_parent.Position + new Vector3(Chunk.Width, 0, Chunk.Width))
            );

            ProcessBorder(Mesh, borders.BackRightCorner,
                World.GetChunkAt(_parent.Position + new Vector3(Chunk.Width, 0, -Chunk.Width))
            );
            return Mesh;
        }

        private void ProcessBorder(VertexData Mesh, Vector3[] Border, Chunk Neighbour)
        {
            if(Neighbour == null || Neighbour.Lod == _parent.Lod) return;

            for (var i = 0; i < Border.Length; i++)
            {
                for (var k = 0; k < Mesh.Vertices.Count; k++)
                {
                    if(Mesh.Vertices[k] == Border[i])
                        Mesh.Normals[k] = new Vector3(Mesh.Normals[k].X, Mesh.Normals[k].Y, 0);
                }
            }
        }
    }
}
