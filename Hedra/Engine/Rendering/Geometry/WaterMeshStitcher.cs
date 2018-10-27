using System.Collections.Generic;
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

        public void Process(VertexData Base, VertexData Border, Vector3 Start, Vector3 End, Vector3 BorderStart, Vector3 BorderEnd)
        {
            var baseBorders = _borderDetector.Process(Base, Start, End);
            var connectBorder = _borderDetector.Process(Border, BorderStart, BorderEnd);

            ProcessBorder(baseBorders.FrontBorder, connectBorder.FrontBorder, Base, Border);
            ProcessBorder(baseBorders.RightBorder, connectBorder.RightBorder, Base, Border);
            ProcessBorder(baseBorders.BackBorder, connectBorder.BackBorder, Base, Border);
            ProcessBorder(baseBorders.LeftBorder, connectBorder.LeftBorder, Base, Border);

            /* Corners */

            ProcessBorder(baseBorders.FrontLeftCorner, connectBorder.FrontLeftCorner, Base, Border);
            ProcessBorder(baseBorders.BackLeftCorner, connectBorder.BackLeftCorner, Base, Border);
            ProcessBorder(baseBorders.BackRightCorner, connectBorder.BackRightCorner, Base, Border);
            ProcessBorder(baseBorders.FrontRightCorner, connectBorder.FrontRightCorner, Base, Border);
        }

        private static void ProcessBorder(Vector3[] Base, Vector3[] Border, VertexData BaseObject, VertexData BorderObject)
        {
            for (var i = 0; i < Border.Length; i++)
            {
                var nearest = FindNearest(Base, Border[i]);
                if(nearest == Vector3.Zero) continue;
                var borderIndex = BorderObject.Vertices.IndexOf(Border[i]);
                BorderObject.Vertices[borderIndex] = nearest;
                BorderObject.Normals[borderIndex] = 
                    new Vector3(BorderObject.Normals[borderIndex].X, BorderObject.Normals[borderIndex].Y, 0);

                StopAll(BaseObject, nearest);
            }
        }

        private static void StopAll(VertexData Base, Vector3 Vertex)
        {
            for (var i = 0; i < Base.Vertices.Count; i++)
            {
                if(Base.Vertices[i] == Vertex)
                    Base.Normals[i] = new Vector3(Base.Normals[i].X, Base.Normals[i].Y, 0);
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
