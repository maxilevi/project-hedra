using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering;
using Hedra.Framework;
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

        public void RemoveEdge(Waypoint A, Waypoint B)
        {
            _adjacencyList[A].Remove(B);
            _adjacencyList[B].Remove(A);
        }
        
        public Waypoint[] Adjacent(Waypoint A)
        {
            return _adjacencyList[A].ToArray();
        }
        
        public void Draw()
        {
            var vertices = Vertices;
            for (var i = 0; i < vertices.Length; ++i)
            {
                BasicGeometry.DrawPoint(vertices[i].Position, Colors.Red, 6f);
            }
            var edges = Edges;
            for (var i = 0; i < edges.Length; ++i)
            {
                BasicGeometry.DrawLine(edges[i].One.Position, edges[i].Two.Position, Colors.Red, 2f);
            }
        }

        private Pair<Waypoint, Waypoint>[] GetEdges()
        {
            var edges = new HashSet<Pair<Waypoint, Waypoint>>();
            var vertices = Vertices;
            for (var i = 0; i < vertices.Length; ++i)
            {
                var adjacent = Adjacent(vertices[i]);
                for (var j = 0; j < adjacent.Length; ++j)
                {
                    var pair = new Pair<Waypoint, Waypoint>(vertices[i], adjacent[j]);
                    if(!edges.Contains(pair) && !edges.Contains(pair.Inverted()))
                        edges.Add(pair);
                }
            }

            return edges.ToArray();
        }

        public Waypoint GetNearestVertex(Vector3 Point)
        {
            var vertices = Vertices;
            Waypoint point = default;
            float dist = float.MaxValue;
            for (var i = 0; i < vertices.Length; ++i)
            {
                var newDist = (vertices[i].Position - Point).LengthSquared();
                if (newDist < dist)
                {
                    point = vertices[i];
                    dist = newDist;
                }
            }
            return point;
        }

        private Waypoint[] ReconstructPath(Dictionary<Waypoint, Waypoint> Parents, Waypoint Source, Waypoint Target)
        {
            var path = new Stack<Waypoint>();
            var current = Target;
            while (current.Position != Source.Position)
            {
                path.Push(current);
                current = Parents[current];
            }
            return path.ToArray();
        }
        
        public Waypoint[] GetShortestPath(Waypoint Source, Waypoint Target, out bool CanReach)
        {
            CanReach = true;
            if (Source.Position == Target.Position) return new[] { Source };
            var parents = new Dictionary<Waypoint, Waypoint>();
            var queue = new Queue<Waypoint>();
            parents.Add(Source, default);
            queue.Enqueue(Source);
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var w in Adjacent(v))
                {
                    if(parents.ContainsKey(w)) continue;
                    parents.Add(w, v);
                    if (w.Position == Target.Position) return ReconstructPath(parents, Source, Target);
                    queue.Enqueue(w);
                }
            }

            CanReach = false;
            return new Waypoint[0];
        }
        
        public Pair<Waypoint, Waypoint>[] Edges => GetEdges();

        public Waypoint[] Vertices => _adjacencyList.Keys.ToArray();
    }

    public struct Waypoint
    {
        public Vector3 Position;
    }
}