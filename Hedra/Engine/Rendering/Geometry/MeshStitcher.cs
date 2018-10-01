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
        
        public void Process(VertexData Base, VertexData Border, Vector3 Start, Vector3 End, Vector3 BorderStart, Vector3 BorderEnd)
        {
            var baseBorders = _borderDetector.Process(Base, Start, End);
            var connectBorder = _borderDetector.Process(Border, BorderStart, BorderEnd);

            /* Corners */

            ProcessBorder(baseBorders.FrontLeftCorner, connectBorder.FrontLeftCorner, Border);
            ProcessBorder(baseBorders.BackLeftCorner, connectBorder.BackLeftCorner, Border);
            ProcessBorder(baseBorders.BackRightCorner, connectBorder.BackRightCorner, Border);
            ProcessBorder(baseBorders.FrontRightCorner, connectBorder.FrontRightCorner, Border);
            
            ProcessBorder(baseBorders.FrontBorder, connectBorder.FrontBorder, Border);
            ProcessBorder(baseBorders.RightBorder, connectBorder.RightBorder, Border);
            ProcessBorder(baseBorders.BackBorder, connectBorder.BackBorder, Border);
            ProcessBorder(baseBorders.LeftBorder, connectBorder.LeftBorder, Border);
        }

        private static void ProcessBorder(Vector3[] Base, Vector3[] Border, VertexData BorderObject)
        {
            // Instead we should sort by closer and not filled.
            for (var i = 0; i < Border.Length; i++)
            {
                BorderObject.Vertices[BorderObject.Vertices.IndexOf(Border[i])] = FindNearest(Base, Border[i]);
            }
        }

        /// <summary>
        /// Finds the nearest vertex in a rray, in order to stitch them.
        /// </summary>
        /// <param name="Base">Array of vertices to look into.</param>
        /// <param name="Vertex">Search candidate</param>
        /// <returns>Return the nearest vertex or Vector3.Zero if none found.</returns>
        private static Vector3 FindNearest(Vector3[] Base, Vector3 Vertex)
        {
            var dist = float.MaxValue;
            var point = Vector3.Zero;
            for (var i = 0; i < Base.Length; i++)
            {
                var borderPoint = Base[i];
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