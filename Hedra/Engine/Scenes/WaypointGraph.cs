using System;
using System.Buffers;
using System.Collections.Concurrent;
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
        private Dictionary<Waypoint, HashSet<Waypoint>> _adjacencyList;
        private readonly ObjectPool<Dictionary<Waypoint, Waypoint>> _parentsPool;
        private readonly ObjectPool<Queue<Waypoint>> _queuePool;

        public WaypointGraph()
        {
            _adjacencyList = new Dictionary<Waypoint, HashSet<Waypoint>>();
            _parentsPool = new ObjectPool<Dictionary<Waypoint, Waypoint>>(() => new Dictionary<Waypoint, Waypoint>());
            _queuePool = new ObjectPool<Queue<Waypoint>>(() => new Queue<Waypoint>());
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
        
        public virtual void Draw()
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

        public virtual Waypoint GetNearestVertex(Vector3 Point)
        {
            return GetNearestVertex(Point, out _);
        }
        
        public Waypoint GetNearestVertex(Vector3 Point, out float Distance)
        {
            return GetNearestVertex(Point, W => false, out Distance);
        }

        protected Waypoint GetNearestVertex(Vector3 Point, Func<Waypoint, bool> Filter, out float Distance)
        {
            var vertices = Vertices;
            Waypoint point = default;
            Distance = float.MaxValue;
            for (var i = 0; i < vertices.Length; ++i)
            {
                if(Filter(vertices[i])) continue;
                var newDist = (vertices[i].Position - Point).LengthSquared();
                if (newDist < Distance)
                {
                    point = vertices[i];
                    Distance = newDist;
                }
            }

            Distance = (float)Math.Sqrt(Distance);
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
            var parents = _parentsPool.GetObject();
            var queue = _queuePool.GetObject();

            void Cleanup()
            {
                queue.Clear();
                parents.Clear();
                _queuePool.PutObject(queue);
                _parentsPool.PutObject(parents);
            }
            
            parents.Add(Source, default);
            queue.Enqueue(Source);
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var w in Adjacent(v))
                {
                    if(parents.ContainsKey(w)) continue;
                    parents.Add(w, v);
                    if (w.Position == Target.Position)
                    {
                        var path = ReconstructPath(parents, Source, Target);
                        Cleanup();
                        return path;
                    }
                    queue.Enqueue(w);
                }
            }

            CanReach = false;
            Cleanup();
            return new Waypoint[0];
        }

        public virtual void Clear()
        {
            _adjacencyList.Clear();
        }
        
                
        public virtual void MergeGraph(WaypointGraph Graph, int MergeStep)
        {
            var set = new Dictionary<Vector3, Waypoint>();
            foreach (var v in Vertices)
            {
                set.Add(
                    new Vector3((int) (v.Position.X / MergeStep), (int) (v.Position.Y / MergeStep),
                        (int) (v.Position.Z / MergeStep)), v);
            }

            /* Doing lookups in a smaller set is always faster */
            var vertices = Graph.Vertices;
            var canBeConvex = false;
            for (var i = 0; i < vertices.Length && !canBeConvex; ++i)
            {
                var isContained = set.ContainsKey(vertices[i].Position);
                canBeConvex |= isContained;
            }
            if(!canBeConvex) return;

            foreach (var v in vertices)
            {
                var n = v;
                var quantized = new Vector3((int) (v.Position.X / MergeStep), (int) (v.Position.Y / MergeStep), (int) (v.Position.Z / MergeStep));
                if (set.TryGetValue(quantized, out var m)) n = m;
                foreach (var w in Graph.Adjacent(v))
                {
                    AddEdge(n, w);
                }
            }
        }

        public void AddGraph(WaypointGraph Graph)
        {
            foreach (var v in Graph.Vertices)
            {
                foreach (var w in Graph.Adjacent(v))
                {
                    AddEdge(v, w);
                }
            }
        }

        public void Copy(WaypointGraph Graph)
        {
            _adjacencyList = Graph._adjacencyList;
        }

        public WaypointGraph Clone()
        {
            var graph = new WaypointGraph
            {
                _adjacencyList = new Dictionary<Waypoint, HashSet<Waypoint>>(_adjacencyList),
            };
            return graph;
        }
        
        public Pair<Waypoint, Waypoint>[] Edges => GetEdges();

        public Waypoint[] Vertices => _adjacencyList.Keys.ToArray();
    }

    public struct Waypoint
    {
        public Vector3 Position;
    }
}