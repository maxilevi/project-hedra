/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/05/2016
 * Time: 05:21 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Numerics;

namespace Hedra.Rendering
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of VertexData.
    /// </summary>
    public class VertexData : BaseVertexData<VertexData>, IDisposable, IModelData
    {
        private readonly Dictionary<Vector3, int> _points;
        private List<Vector4> _colors;
        private List<float> _extradata;
        private List<VertexData> _groups;
        private List<uint> _indices;
        private List<Vector3> _normals;
        private List<Vector3> _vertices;

        static VertexData()
        {
            Empty = new VertexData();
        }

        public VertexData()
        {
            _vertices = new List<Vector3>();
            _colors = new List<Vector4>();
            _normals = new List<Vector3>();
            _indices = new List<uint>();
            _extradata = new List<float>();
            _points = new Dictionary<Vector3, int>();
        }

        public string Name { get; set; }
        public VertexData Original { get; set; }
        public bool UseCache { get; set; }
        public static VertexData Empty { get; }

        public bool IsEmpty => Vertices.Count == 0
                               && Indices.Count == 0
                               && Normals.Count == 0
                               && Colors.Count == 0
                               && Extradata.Count == 0;

        public int SizeInBytes => Indices.Count * sizeof(uint)
                                  + Vertices.Count * HedraSize.Vector3
                                  + Normals.Count * HedraSize.Vector3
                                  + Colors.Count * HedraSize.Vector4
                                  + Extradata.Count * sizeof(float);

        public bool IsClone => Original != null;

        public override List<Vector3> Vertices
        {
            get => _vertices;
            set => _vertices = value;
        }

        public override List<Vector3> Normals
        {
            get => _normals;
            set => _normals = value;
        }

        public override List<Vector4> Colors
        {
            get => _colors;
            set => _colors = value;
        }

        public override List<uint> Indices
        {
            get => _indices;
            set => _indices = value;
        }

        public override List<float> Extradata
        {
            get => _extradata;
            set => _extradata = value;
        }

        public void Dispose()
        {
            Extradata.Clear();
            Indices.Clear();
            Normals.Clear();
            Vertices.Clear();
            Indices.Clear();
            Extradata.Clear();
        }

        uint[] IModelData.Indices => Indices.ToArray();

        public VertexData AddWindValues(float Scalar = 1)
        {
            if (!HasExtradata) Extradata = Enumerable.Repeat(0.01f, Vertices.Count).ToList();
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, Scalar);
            ApplyRecursively(V => V.AddWindValues(Scalar));
            return this;
        }

        public VertexData AddWindValues(Vector4 ColorFilter, float Scalar = 1)
        {
            if (!HasExtradata) Extradata = Enumerable.Repeat(0.01f, Vertices.Count).ToList();
            MeshOperations.AddWindValues(Vertices, Colors, Extradata, ColorFilter, Scalar);
            ApplyRecursively(V => V.AddWindValues(ColorFilter, Scalar));
            return this;
        }

        public VertexData FillExtraData(float Value)
        {
            Extradata.Clear();
            for (var i = 0; i < Vertices.Count; i++) Extradata.Add(Value);
            ApplyRecursively(V => V.FillExtraData(Value));
            return this;
        }

        public VertexData Translate(Vector3 Position)
        {
            Transform(Matrix4x4.CreateTranslation(Position));
            return this;
        }

        public VertexData GraduateColor(Vector3 Direction)
        {
            MeshOperations.GraduateColor(Vertices, Colors, Direction);
            ApplyRecursively(V => V.GraduateColor(Direction));
            return this;
        }


        public VertexData Transform(Matrix4x4 Mat)
        {
            MeshOperations.Transform(Vertices, Normals, Mat);
            ApplyRecursively(V => V.Transform(Mat));
            return this;
        }

        public VertexData AverageCenter()
        {
            var avg = Vertices.Aggregate((V1, V2) => V1 + V2) / Vertices.Count;
            Vertices = Vertices.Select(V => V - avg).ToList();
            return this;
        }

        public VertexData Center()
        {
            var min = new Vector3(
                SupportPoint(-Vector3.UnitX).X,
                SupportPoint(-Vector3.UnitY).Y,
                SupportPoint(-Vector3.UnitZ).Z
            );
            var renderCenter = new Vector3(
                SupportPoint(Vector3.UnitX).X - min.X,
                SupportPoint(Vector3.UnitY).Y - min.Y,
                SupportPoint(Vector3.UnitZ).Z - min.Z
            ) * .5f;
            Vertices = Vertices.Select(V => V - (min + renderCenter)).ToList();
            ApplyRecursively(V => V.Center());
            return this;
        }

        public void Optimize(IAllocator Allocator)
        {
            MeshOperations.Optimize(Allocator, _indices, _vertices, _normals, _colors, _extradata);
        }

        public VertexData[] Ungroup()
        {
            return MeshAnalyzer.GetConnectedComponents(this);
        }

        public VertexData RotateX(float EulerAngle)
        {
            return Transform(Matrix4x4.CreateRotationX(EulerAngle * Mathf.Radian));
        }

        public VertexData RotateY(float EulerAngle)
        {
            return Transform(Matrix4x4.CreateRotationY(EulerAngle * Mathf.Radian));
        }

        public VertexData RotateZ(float EulerAngle)
        {
            return Transform(Matrix4x4.CreateRotationZ(EulerAngle * Mathf.Radian));
        }

        public VertexData Scale(Vector3 Scalar)
        {
            return Transform(Matrix4x4.CreateScale(Scalar));
        }

        public void Paint(Vector4 Color)
        {
            MeshOperations.PaintMesh(Colors, Color);
            ApplyRecursively(V => V.Paint(Color));
        }

        public CompressedVertexData AsCompressed()
        {
            return CompressedVertexData.FromVertexData(this);
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
            if (HasLod)
                for (var i = 1; i < 4; ++i)
                {
                    var iterator = (int)Math.Pow(2, i);
                    data.AddLOD(Get(iterator, false)?.ToInstanceData(Transformation), iterator);
                }

            CacheManager.Check(data);
            return data;
        }

        public VertexData Clone()
        {
            var data = new VertexData
            {
                Indices = new List<uint>(Indices),
                Vertices = new List<Vector3>(Vertices),
                Colors = new List<Vector4>(Colors),
                Normals = new List<Vector3>(Normals),
                Extradata = new List<float>(Extradata),
                Original = Original ?? this
            };
            if (data.Original.HasLod)
                for (var i = 1; i < 4; ++i)
                {
                    var iterator = (int)Math.Pow(2, i);
                    data.AddLOD(Get(iterator, false)?.Clone(), iterator);
                }

            return data;
        }

        public NativeVertexData NativeClone(IAllocator Allocator)
        {
            return new NativeVertexData(Allocator, Indices, Vertices, Normals, Colors, Extradata)
            {
                Original = Original ?? this,
                Name = Name
            };
        }

        public VertexData ShallowClone()
        {
            return Clone();
        }

        public void Color(Vector4 OriginalColor, Vector4 ReplacementColor)
        {
            MeshOperations.ColorMesh(Colors, OriginalColor, ReplacementColor);
            ApplyRecursively(V => V.Color(OriginalColor, ReplacementColor));
        }

        public static VertexData operator +(VertexData V1, VertexData V2)
        {
            if (V1?.Indices == null) return V2.Clone();
            var v3 = V2.Clone();
            for (var i = 0; i < v3.Indices.Count; i++) v3.Indices[i] += (uint)V1.Vertices.Count;
            V1.Vertices.AddRange(v3.Vertices);
            V1.Colors.AddRange(v3.Colors);
            V1.Normals.AddRange(v3.Normals);
            V1.Indices.AddRange(v3.Indices);
            V1.Extradata.AddRange(v3.Extradata);

            v3.Dispose();
            return V1;
        }

        /* Do not remove. Used in python scripts */
        public static VertexData Load(string Path, Vector3 Scale)
        {
            return AssetManager.LoadModel(Path, Scale);
        }
    }
}