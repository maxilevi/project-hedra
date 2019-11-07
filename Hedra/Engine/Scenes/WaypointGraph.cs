using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering;
using Hedra.Game;
using Hedra.Rendering;

namespace Hedra.Engine.Scenes
{
    /// <summary>
    /// Undirected graph
    /// </summary>
    public class WaypointGraph
    {
        private readonly Dictionary<Waypoint, HashSet<Waypoint>> _adjacencyList;

        public WaypointGraph()
        {
            _adjacencyList = new Dictionary<Waypoint, HashSet<Waypoint>>();
        }

        public void AddVertex(Waypoint A)
        {
            if(!_adjacencyList.ContainsKey(A))
                _adjacencyList.Add(A, new HashSet<Waypoint>());
        }

        public void AddEdge(Waypoint A, Waypoint B)
        {
            AddVertex(A);
            AddVertex(B);
            _adjacencyList[A].Add(B);
            _adjacencyList[B].Add(A);
        }

        public Waypoint[] Adjacent(Waypoint A)
        {
            return _adjacencyList[A].ToArray();
        }

        public void Draw()
        {
            if (!GameSettings.DebugNavMesh) return;
            var vertices = Vertices;
            var visited = new HashSet<Waypoint>();
            for (var i = 0; i < vertices.Length; ++i)
            {
                var v = vertices[i];
                if(visited.Contains(v)) continue;
                
                visited.Add(v);
                BasicGeometry.DrawPoint(v.Position, Colors.Red);
                var adjacents = Adjacent(v);
                for (var j = 0; j < adjacents.Length; ++j)
                {
                    var w = adjacents[j];
                    if(visited.Contains(w)) continue;
                    
                    visited.Add(w);
                    BasicGeometry.DrawLine(v.Position, w.Position, Colors.Red, 2f);
                    BasicGeometry.DrawPoint(w.Position, Colors.Red);
                }
            }
        }

        public Waypoint[] Vertices => _adjacencyList.Keys.ToArray();
    }

    public struct Waypoint
    {
        public Vector3 Position;
        public float Size;
    }
}