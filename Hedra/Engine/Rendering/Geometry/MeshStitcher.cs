using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class MeshStitcher
    {
        private readonly ChunkMeshBorderDetector _borderDetector;

        public MeshStitcher()
        {
            _borderDetector = new ChunkMeshBorderDetector();
        }
        
        public VertexData Process(VertexData Terrain, Vector3 Position, Vector3 Size)
        {
            var borders = 
            return Terrain;
        }
    }
}