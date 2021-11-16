using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering
{
    public class NativeVertexData
    {
        private const string DefaultName = "NativeVertexData";

        public NativeVertexData(IAllocator Allocator, ICollection<uint> Indices, ICollection<Vector3> Vertices,
            ICollection<Vector3> Normals, ICollection<Vector4> Colors, ICollection<float> Extradata)
        {
            this.Vertices = new NativeList<Vector3>(Allocator, Vertices);
            this.Colors = new NativeList<Vector4>(Allocator, Colors);
            this.Normals = new NativeList<Vector3>(Allocator, Normals);
            this.Indices = new NativeList<uint>(Allocator, Indices);
            this.Extradata = new NativeList<float>(Allocator, Extradata);
        }

        public NativeVertexData(IAllocator Allocator)
        {
            Vertices = new NativeList<Vector3>(Allocator);
            Colors = new NativeList<Vector4>(Allocator);
            Normals = new NativeList<Vector3>(Allocator);
            Indices = new NativeList<uint>(Allocator);
            Extradata = new NativeList<float>(Allocator);
        }

        public string Name { get; set; } = DefaultName;
        public bool IsClone => Original != null;
        public VertexData Original { get; set; }
        public bool HasExtradata => Extradata.Count != 0;

        public NativeList<Vector3> Vertices { get; }

        public NativeList<Vector3> Normals { get; }

        public NativeList<Vector4> Colors { get; }

        public NativeList<uint> Indices { get; }

        public NativeList<float> Extradata { get; }

        public bool IsEmpty => Vertices.Count == 0
                               && Indices.Count == 0
                               && Normals.Count == 0
                               && Colors.Count == 0
                               && Extradata.Count == 0;

        public void AssertTriangulated()
        {
            if (Indices.Count % 3 != 0)
                throw new ArgumentOutOfRangeException(
                    $"ModelData with '{Indices.Count}' indices is not triangulated correctly");
        }

        public void Flat(IAllocator Allocator)
        {
            MeshOperations.FlatMesh(Allocator, Indices, Vertices, Normals, Colors, Extradata);
        }

        public void UniqueVertices()
        {
            MeshOperations.UniqueVertices(Indices, Vertices, Normals, Colors, Extradata);
        }

        public void AddWindValues(float Scalar = 1)
        {
            if (!HasExtradata) Extradata.Set(0.01f, Vertices.Count);
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, Scalar);
        }

        public void AddWindValues(Vector4 ColorFilter, float Scalar = 1)
        {
            if (!HasExtradata) Extradata.Set(0.01f, Vertices.Count);
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, ColorFilter, Scalar);
        }

        private void AddWindValues(Vector4 ColorFilter, Vector3 Lowest, Vector3 Highest, float Scalar)
        {
            if (!HasExtradata) Extradata.Set(0.01f, Vertices.Count);
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
            MeshOperations.GraduateColor(Vertices, Colors, Direction);
        }

        public void GraduateColor(Vector3 Direction, float Amount)
        {
            MeshOperations.GraduateColor(Vertices, Colors, Direction, Amount);
        }

        public void Optimize(IAllocator Allocator)
        {
            MeshOperations.Optimize(Allocator, Indices, Vertices, Normals, Colors, Extradata);
        }

        public void Translate(Vector3 Position)
        {
            Transform(Matrix4x4.CreateTranslation(Position));
        }

        public void Transform(Matrix4x4 Transformation)
        {
            MeshOperations.Transform(Vertices, Normals, Transformation);
        }

        public Vector3 SupportPoint(Vector3 Direction)
        {
            return MeshOperations.SupportPoint(Vertices, Colors, Direction);
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
            Vertices.Dispose();
            Colors.Dispose();
            Normals.Dispose();
            Indices.Dispose();
            Extradata.Dispose();
        }

        public VertexData ToVertexData()
        {
            return new VertexData
            {
                Vertices = new List<Vector3>(Vertices),
                Colors = new List<Vector4>(Colors),
                Normals = new List<Vector3>(Normals),
                Extradata = new List<float>(Extradata),
                Indices = new List<uint>(Indices)
            };
        }
    }
}