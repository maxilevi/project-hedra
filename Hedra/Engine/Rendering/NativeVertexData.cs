using System;
using System.Collections.Generic;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.Rendering
{
    public class NativeVertexData
    {
        private const string DefaultName = "NativeVertexData";
        public string Name { get; set; } = DefaultName;
        public bool IsClone => Original != null;
        public VertexData Original { get; set; }
        public bool HasExtradata => _extradata.Count != 0;
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

        public void AssertTriangulated()
        {
            if(_indices.Count % 3 != 0)
                throw new ArgumentOutOfRangeException($"ModelData with '{_indices.Count}' indices is not triangulated correctly");
        }
        
        public void Flat(IAllocator Allocator)
        {
            MeshOperations.FlatMesh(Allocator, _indices, _vertices, _normals, _colors, _extradata);
        }

        public void UniqueVertices()
        {
            MeshOperations.UniqueVertices(_indices, _vertices, _normals, _colors, _extradata);
        }
        
        public void AddWindValues(float Scalar = 1)
        {
            if(!HasExtradata) Extradata.Set(0.01f, Vertices.Count);
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, Scalar);
        }
        
        public void AddWindValues(Vector4 ColorFilter, float Scalar = 1)
        {
            if(!HasExtradata) Extradata.Set(0.01f, Vertices.Count);
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, ColorFilter, Scalar);
        }

        private void AddWindValues(Vector4 ColorFilter, Vector3 Lowest, Vector3 Highest, float Scalar)
        {
            if(!HasExtradata) Extradata.Set(0.01f, Vertices.Count);
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, ColorFilter, Lowest, Highest, Scalar);
        }

        public void Paint(Vector4 Color)
        {
            MeshOperations.PaintMesh(Colors, Color);
        }

        public void Color(Vector4 Original, Vector4 Replacement)
        {
            MeshOperations.ColorMesh(Colors, Original, Replacement);
        }

        public void GraduateColor(Vector3 Direction)
        {
            MeshOperations.GraduateColor(_vertices, _colors, Direction);
        }
        
        public void GraduateColor(Vector3 Direction, float Amount)
        {
            MeshOperations.GraduateColor(_vertices, _colors, Direction, Amount);
        }

        public void Optimize(IAllocator Allocator)
        {
            MeshOperations.Optimize(Allocator, _indices, _vertices, _normals, _colors, _extradata);
        }
        
        public void Translate(Vector3 Position)
        {
            Transform(Matrix4x4.CreateTranslation(Position));
        }
        
        public void Transform(Matrix4x4 Transformation)
        {
            MeshOperations.Transform(_vertices, _normals, Transformation);
        }
        
        public Vector3 SupportPoint(Vector3 Direction)
        {
            return MeshOperations.SupportPoint( _vertices, _colors, Direction);
        }
        
        public InstanceData ToInstanceData(Matrix4x4 Transformation)
        {
            if (!IsClone)
                throw new ArgumentOutOfRangeException("VertexData needs to be a clone.");
            var data = new InstanceData
            {
                ExtraData = Extradata,
                OriginalMesh = Original,
                Colors = Colors,
                TransMatrix = Transformation
            };
            CacheManager.Check(data);
            return data;
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
        
        public bool IsEmpty => Vertices.Count == 0
                               && Indices.Count == 0
                               && Normals.Count == 0
                               && Colors.Count == 0
                               && Extradata.Count == 0;
    }
}