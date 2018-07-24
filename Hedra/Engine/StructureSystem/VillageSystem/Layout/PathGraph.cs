using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Layout
{
    internal class PathGraph
    {
        private readonly List<PathEdge> _edges;
        private readonly List<PathVertex> _vertices;

        public PathGraph()
        {
            _edges = new List<PathEdge>();
            _vertices = new List<PathVertex>();
        }

        public void AddVertex(PathVertex Vertex)
        {
            if(_vertices.Contains(Vertex))throw new ArgumentException($"Vertex already exists in the graph.");
            if(_vertices.Any(V => V.Point == Vertex.Point))throw new ArgumentException($"Vertex with the same position already exists in the graph.");
            _vertices.Add(Vertex);
        }
        
        public void AddEdge(PathEdge Edge)
        {
            if(!_vertices.Contains(Edge.Origin)) throw new ArgumentException($"Edge contains a vertex that doesnt exist in the graph.");
            if(!_vertices.Contains(Edge.End)) throw new ArgumentException($"Edge contains a vertex that doesnt exist in the graph.");
            if(_edges.Contains(Edge)) throw new ArgumentException($"Edge already exists in the graph.");
            _edges.Add(Edge);
        }

        public void Smooth(int Steps)
        {
            for (var k = 0; k < Steps; k++)
            {
                for (var i = 0; i < _edges.Count; i++)
                {
                    this.SmoothEdge(_edges[i]);
                }
            }
        }

        public void Collapse()
        {
            
        }
        
        private void SmoothEdge(PathEdge Edge)
        {
            var p0 = Edge.Origin;
            var p1 = Edge.End;

            var q0 = new Vector3(0.75f * p0.X + 0.25f * p1.X, 0.75f * p0.Y + 0.25f * p1.Y, 0.75f * p0.Z + 0.25f * p1.Z);
            var r0 = new Vector3(0.25f * p0.X + 0.75f * p1.X, 0.25f * p0.Y + 0.75f * p1.Y, 0.25f * p0.Z + 0.75f * p1.Z);
            var vertex0 = new PathVertex
            {
                Point = q0
            };
            var vertex1 = new PathVertex
            {
                Point = r0
            };
            this.AddVertex(vertex0);
            this.AddVertex(vertex1);
            var edge0 = new PathEdge
            {
                Origin = p0,
                End = vertex0,
            };
            Edge.Origin = vertex0;
            Edge.End = vertex1;
            var edge1 = new PathEdge
            {
                Origin = vertex1,
                End = p1,
            };
            this.AddEdge(edge0);
            this.AddEdge(edge1);
        }

        public PathEdge[] Edges => _edges.ToArray();
    }
}