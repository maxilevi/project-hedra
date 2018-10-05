using System.Collections.Generic;
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
            
            ProcessBorder(baseBorders.FrontBorder, connectBorder.FrontBorder, Border, Base);
            ProcessBorder(baseBorders.RightBorder, connectBorder.RightBorder, Border, Base);
            ProcessBorder(baseBorders.BackBorder, connectBorder.BackBorder, Border, Base);
            ProcessBorder(baseBorders.LeftBorder, connectBorder.LeftBorder, Border, Base);

            /* Corners */
            
            ProcessBorder(baseBorders.FrontLeftCorner, connectBorder.FrontLeftCorner, Border, Base);
            ProcessBorder(baseBorders.BackLeftCorner, connectBorder.BackLeftCorner, Border, Base);
            ProcessBorder(baseBorders.BackRightCorner, connectBorder.BackRightCorner, Border, Base);
            ProcessBorder(baseBorders.FrontRightCorner, connectBorder.FrontRightCorner, Border, Base);
        }

        private static void ProcessBorder(Vector3[] Base, Vector3[] Border, VertexData BorderObject, VertexData BaseObject)
        {
            /*for (var i = 0; i < Base.Length; i++)
            {
                var nearesBorder = FindNearest(Border, Base[i]);
                BaseObject.Vertices[BaseObject.Vertices.IndexOf(Base[i])] = nearesBorder;
            }*/
            for (var i = 0; i < Border.Length; i++)
            {
                var nearestBase = FindNearest(Base, Border[i]);
                BorderObject.Vertices[BorderObject.Vertices.IndexOf(Border[i])] = nearestBase;
            }
        }

        /// <summary>
        /// Finds the nearest vertex in a rray, in order to stitch them.
        /// </summary>
        /// <param name="Base">Array of vertices to look into.</param>
        /// <param name="Vertex">Search candidate</param>
        /// <returns>Return the nearest vertex or Vector3.Zero if none found.</returns>
        private static Vector3 FindNearest(IList<Vector3> Base, Vector3 Vertex)
        {
            var dist = float.MaxValue;
            var point = Vector3.Zero;
            for (var i = 0; i < Base.Count; i++)
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