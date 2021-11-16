using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Layout
{
    public class PathGraph
    {
        private readonly List<PathEdge> _edges;
        private readonly List<PathVertex> _vertices;

        public PathGraph(Vector3 Position)
        {
            this.Position = Position;
            _edges = new List<PathEdge>();
            _vertices = new List<PathVertex>();
        }

        public Vector3 Position { get; set; }

        public PathEdge[] Edges => _edges.ToArray();
        public PathVertex[] Vertices => _vertices.ToArray();

        public void AddVertex(params PathVertex[] Vertices)
        {
            for (var i = 0; i < Vertices.Length; i++)
            {
                if (_vertices.Contains(Vertices[i])) throw new ArgumentException("Vertex already exists in the graph.");
                if (_vertices.Any(V => V.Point == Vertices[i].Point))
                    throw new ArgumentException("Vertex with the same position already exists in the graph.");
                _vertices.Add(Vertices[i]);
            }
        }

        public void AddEdge(params PathEdge[] Edges)
        {
            for (var i = 0; i < Edges.Length; i++)
            {
                if (!_vertices.Contains(Edges[i].Origin))
                    throw new ArgumentException("Edge contains a vertex that doesnt exist in the graph.");
                if (!_vertices.Contains(Edges[i].End))
                    throw new ArgumentException("Edge contains a vertex that doesnt exist in the graph.");
                if (_edges.Contains(Edges[i])) throw new ArgumentException("Edge already exists in the graph.");
                _edges.Add(Edges[i]);
            }
        }

        public int Degree(PathVertex Vertex)
        {
            var degree = 0;
            for (var i = 0; i < _edges.Count; i++)
                if (_edges[i].End == Vertex || _edges[i].Origin == Vertex)
                    degree++;
            return degree;
        }

        public void AddAttribute(string Name, Func<PathVertex, object> Func)
        {
            for (var i = 0; i < _vertices.Count; i++) _vertices[i].Attributes.Set(Name, Func(_vertices[i]));
        }

        public PathVertex Find(Predicate<PathVertex> IsKey)
        {
            return _vertices.Find(IsKey);
        }

        public void Smooth(int Steps)
        {
            for (var k = 0; k < Steps; k++)
            {
                var originalEdges = Edges;
                for (var i = 0; i < originalEdges.Length; i++) SmoothEdge(originalEdges[i]);
            }
        }

        public void Collapse()
        {
        }

        private void SmoothEdge(PathEdge Edge)
        {
            var p0 = Edge.Origin;
            var p1 = Edge.End;

            var q0 = new Vector3(
                0.75f * (p0.X - Position.X) + 0.25f * (p1.X - Position.X),
                0.75f * (p0.Y - Position.Y) + 0.25f * (p1.Y - Position.Y),
                0.75f * (p0.Z - Position.Z) + 0.25f * (p1.Z - Position.Z)
            );
            var r0 = new Vector3(
                0.25f * (p0.X - Position.X) + 0.75f * (p1.X - Position.X),
                0.25f * (p0.Y - Position.Y) + 0.75f * (p1.Y - Position.Y),
                0.25f * (p0.Z - Position.Z) + 0.75f * (p1.Z - Position.Z)
            );
            var vertex0 = new PathVertex
            {
                Point = q0 + Position
            };
            var vertex1 = new PathVertex
            {
                Point = r0 + Position
            };
            AddVertex(vertex0);
            AddVertex(vertex1);
            var edge0 = new PathEdge
            {
                Origin = p0,
                End = vertex0
            };
            Edge.Origin = vertex0;
            Edge.End = vertex1;
            var edge1 = new PathEdge
            {
                Origin = vertex1,
                End = p1
            };
            AddEdge(edge0);
            AddEdge(edge1);
        }
    }
}