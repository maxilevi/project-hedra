using System.Collections.Generic;
using Hedra.Engine.Rendering;
using Hedra.Game;

namespace Hedra.Engine.Scenes
{
    /// <summary>
    /// Undirected graph
    /// </summary>
    public class WaypointGraph
    {
        public Dictionary<Waypoint, HashSet<Waypoint>> _adjacencyList;

        public WaypointGraph()
        {
            _adjacencyList = new Dictionary<Waypoint, HashSet<Waypoint>>();
        }

        public void AddWaypoint()
        {
            
        }

        public void AddEdge(Waypoint A, Waypoint B)
        {
            if(!_adjacencyList.ContainsKey(A)) _adjacencyList.Add(A, new HashSet<Waypoint>());
            if(!_adjacencyList.ContainsKey(A)) _adjacencyList.Add(A, new HashSet<Waypoint>());
            _adjacencyList[A].Add(B);
            _adjacencyList[B].Add(A);
        }

        public void Draw()
        {
            if (GameSettings.DebugNavMesh)
            {
                
            }
        }
    }

    public struct Waypoint
    {
        
    }
}