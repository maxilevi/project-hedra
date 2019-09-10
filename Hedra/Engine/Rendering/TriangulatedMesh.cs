using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using OpenTK;
using UnityMeshSimplifier;

namespace Hedra.Engine.Rendering
{
    public class TriangulatedMesh
    {
        private readonly List<Vector3> _vertices;
        private readonly List<Vector4> _colors;
        private readonly List<Vector3> _normals;
        private readonly List<FastMeshSimplifier.Triangle> _triangles;

        public TriangulatedMesh()
        {
            _vertices = new List<Vector3>();
            _colors = new List<Vector4>();
            _normals = new List<Vector3>();
            _triangles = new List<FastMeshSimplifier.Triangle>();
        }

        public void AddTriangle(Triangle Triangle, Vector4 Color)
        {
            var normal = Vector3.Cross(Triangle.Vertices[1] - Triangle.Vertices[0], Triangle.Vertices[2] - Triangle.Vertices[0]).Normalized();
            _vertices.Add(Triangle.Vertices[0]);
            _vertices.Add(Triangle.Vertices[1]);
            _vertices.Add(Triangle.Vertices[2]);
            
            _normals.Add(normal);
            _normals.Add(normal);
            _normals.Add(normal);
            
            _colors.Add(Color);
            _colors.Add(Color);
            _colors.Add(Color);
            
            _triangles.Add(new FastMeshSimplifier.Triangle(_vertices.Count-1, _vertices.Count-2, _vertices.Count-1));
        }

        public Vector3[] Vertices => _vertices.ToArray();
        public Vector3[] Normals => _normals.ToArray();
        public Vector4[] Colors => _colors.ToArray();
        public FastMeshSimplifier.Triangle[] Triangles => _triangles.ToArray();
        
        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Vertices = Vertices.ToList(),
                Normals = Normals.ToList(),
                Colors = Colors.ToList(),
                Indices = Triangles.SelectMany(T => new []
                {
                    (uint)T.v0,
                    (uint)T.v1,
                    (uint)T.v2
                }).ToList()
            };
        }
    }
}