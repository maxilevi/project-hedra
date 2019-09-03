using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class VillageGraph
    {
        private Vector2 _size;
        private readonly List<Vector2> _vertices;
        private readonly List<GraphEdge> _edges;

        public VillageGraph()
        {
            _vertices = new List<Vector2>();
            _edges = new List<GraphEdge>();
        }

        public void AddSymmetricEdge(Vector2 Start, Vector2 End)
        {
            AddEdge(Start, End);
            AddEdge(End, Start);
        }

        public void AddEdge(Vector2 Start, Vector2 End)
        {
            if (_edges.All(E => _vertices[E.StartIndex] != Start || _vertices[E.EndIndex] != End))
            {
                AddVertex(Start);
                AddVertex(End);
                _edges.Add(new GraphEdge
                {
                    StartIndex = _vertices.IndexOf(Start),
                    EndIndex = _vertices.IndexOf(End)
                });
                CalculateSize();
            }
        }

        public int Degree(int Index)
        {
            return _edges.Count( E => E.StartIndex == Index || E.EndIndex == Index);
        }
        
        public Vector2 FromIndex(int Index)
        {
            return _vertices[Index];
        }

        public int GetIndex(Vector2 Vertex)
        {
            return _vertices.IndexOf(Vertex);
        }
        
        public GraphEdge[] GetEdgesWithVertex(Vector2 Vertex)
        {
            var index = _vertices.IndexOf(Vertex);
            if(index == -1) throw new ArgumentOutOfRangeException($"Vertex {Vertex} does not exist in graph");
            return _edges.Where(E => E.StartIndex == index || E.EndIndex == index).ToArray();
        }
        
        private void AddVertex(Vector2 Position)
        {
            if(!_vertices.Contains(Position))
                _vertices.Add(Position);
        }

        public Vector2 GetNearestVertex(Vector2 Position)
        {
            return _vertices.Aggregate((V1, V2) => (V1 - Position).LengthSquared < (V2 - Position).LengthSquared ? V1 : V2);
        }

        private void CalculateSize()
        {
            _size = new Vector2(
                SupportPoint(Vector2.UnitX).X - SupportPoint(-Vector2.UnitX).X,
                SupportPoint(Vector2.UnitY).Y - SupportPoint(-Vector2.UnitY).Y
            );
        }

        private Vector2 SupportPoint(Vector2 Direction)
        {
            var highest = float.MinValue;
            var support = Vector2.Zero;
            for (var i = 0; i < _vertices.Count; ++i)
            {
                var dot = Vector2.Dot(Direction, _vertices[i]);

                if (dot > highest)
                {
                    highest = dot;
                    support = _vertices[i];
                }
            }
            return support;
        }
        
        public Vector2 Size => _size;
        
        public GraphEdge[] Edges => _edges.ToArray();
        
        public Vector2[] Vertices => _vertices.ToArray();
    }

    public class GraphEdge
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public int GetOtherVertex(int Index)
        {
            return Index == StartIndex
                ? EndIndex
                : Index == EndIndex
                    ? StartIndex
                    : throw new ArgumentOutOfRangeException($"Index '{Index}' is not used in this edge.");
        }
    }
}