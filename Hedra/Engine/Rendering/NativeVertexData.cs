using System.Collections.Generic;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class NativeVertexData
    {
        private const string DefaultName = "NativeVertexData";
        public string Name { get; set; } = DefaultName;
        private readonly NativeList<Vector3> _vertices;
        private readonly NativeList<Vector4> _colors;
        private readonly NativeList<Vector3> _normals;
        private readonly NativeList<uint> _indices;
        private readonly NativeList<float> _extradata;

        public NativeVertexData(IAllocator Allocator, ICollection<uint> Indices, ICollection<Vector3> Vertices, ICollection<Vector3> Normals, ICollection<Vector4> Colors, ICollection<float> Extradata)
        {
            _vertices = new NativeList<Vector3>(Allocator, Vertices);
            _colors = new NativeList<Vector4>(Allocator, Colors);
            _normals = new NativeList<Vector3>(Allocator, Normals);
            _indices = new NativeList<uint>(Allocator, Indices);
            _extradata = new NativeList<float>(Allocator, Extradata);
        }
        
        public NativeVertexData(IAllocator Allocator)
        {
            _vertices = new NativeList<Vector3>(Allocator);
            _colors = new NativeList<Vector4>(Allocator);
            _normals = new NativeList<Vector3>(Allocator);
            _indices = new NativeList<uint>(Allocator);
            _extradata = new NativeList<float>(Allocator);
        }

        public void Flat()
        {
            MeshOperations.FlatMesh(_indices, _vertices, _normals, _colors, _extradata);
        }

        public void UniqueVertices()
        {
            MeshOperations.UniqueVertices(_indices, _vertices, _normals, _colors, _extradata);
        }

        public void GraduateColor(Vector3 Direction)
        {
            MeshOperations.GraduateColor(_vertices, _colors, Direction);
        }
        
        public void GraduateColor(Vector3 Direction, float Amount)
        {
            MeshOperations.GraduateColor(_vertices, _colors, Direction, Amount);
        }
        
        public void Transform(Matrix4 Transformation)
        {
            MeshOperations.Transform(_vertices, _normals, Transformation);
        }
        
        public void SupportPoint(Vector3 Direction)
        {
            MeshOperations.SupportPoint( _vertices, _colors, Direction);
        }
        
        public void Dispose()
        {
            _vertices.Dispose();
            _colors.Dispose();
            _normals.Dispose();
            _indices.Dispose();
            _extradata.Dispose();
        }

        public NativeList<Vector3> Vertices
        {
            get => _vertices;
        }
        public NativeList<Vector3> Normals
        {
            get => _normals;
        }
        public NativeList<Vector4> Colors
        {
            get => _colors;
        }
        public NativeList<uint> Indices
        {
            get => _indices;
        }
        public NativeList<float> Extradata
        {
            get => _extradata;
        }

        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Vertices = new List<Vector3>(_vertices),
                Colors = new List<Vector4>(_colors),
                Normals = new List<Vector3>(_normals),
                Extradata = new List<float>(_extradata),
                Indices = new List<uint>(_indices),
            };
        }
    }
}