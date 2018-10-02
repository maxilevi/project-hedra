using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.Rendering.Geometry
{
    public class MeshStitcher
    {
        private readonly ChunkMeshBorderDetector _borderDetector;
        private VertexData _base;
        private static VertexData _base2;

        public MeshStitcher()
        {
            _borderDetector = new ChunkMeshBorderDetector();
        }
        
        public void Process(VertexData Base, VertexData Border, Vector3 Start, Vector3 End, Vector3 BorderStart, Vector3 BorderEnd)
        {
            _base2 = Base;
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
            var unmatched = new List<Vector3>(Base);
            var basesWithMultipleMatches = new Dictionary<Vector3, int>();
            for (var i = 0; i < Border.Length; i++)
            {
                var nearestBase = FindNearest(Base, Border[i]);
                if (nearestBase == Vector3.Zero) continue;
                BorderObject.Vertices[BorderObject.Vertices.IndexOf(Border[i])] = nearestBase;
                //unmatched.Remove(nearestBase);
                if (basesWithMultipleMatches.ContainsKey(nearestBase))
                    basesWithMultipleMatches[nearestBase]++;
                else
                    basesWithMultipleMatches.Add(nearestBase, 1);
            }
            var candidates = basesWithMultipleMatches.Where( P => P.Value > 1).Select(P => P.Key).ToArray();
            for (var i = 0; i < unmatched.Count; i++)
            {
                var nearestBorder = FindNearest(candidates, unmatched[i]);
                _base2.Colors[_base2.Vertices.IndexOf(nearestBorder)] = new Vector4(1,0,0,1);
                //BorderObject.Vertices[BorderObject.Vertices.IndexOf(nearestBorder)] = unmatched[i];
            }
            /*
            for (var i = 0; i < Base.Length; i++)
            {
                var nearestBorder = FindNearest(unmatched, Base[i]);
                BorderObject.Vertices[BorderObject.Vertices.IndexOf(nearestBorder)] = Base[i];
                unmatched.Remove(nearestBorder);
            }*/
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