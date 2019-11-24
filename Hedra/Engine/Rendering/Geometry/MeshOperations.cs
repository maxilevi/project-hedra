using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Core;
using Microsoft.Scripting.Utils;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Framework;
using Hedra.Numerics;
using Hedra.Framework;

namespace Hedra.Engine.Rendering.Geometry
{
    public class MeshOperations
    {
        public static unsafe void FlatMesh(IList<uint> Indices, IList<Vector3> Vertices, IList<Vector3> Normals,
            IList<Vector4> Colors, IList<float> Extradata)
        {
            var size = Indices.Count * sizeof(uint) + Vertices.Count * HedraSize.Vector3 +
                       Normals.Count * HedraSize.Vector3 + Extradata.Count * sizeof(float) +
                       Colors.Count * HedraSize.Vector4 + Allocator.Kilobyte * 64;
            using (var allocator = new HeapAllocator(Allocator.Megabyte * 8))
            {
                FlatMesh(allocator, Indices, Vertices, Normals, Colors, Extradata);
            }
        }

        public static unsafe void FlatMesh(IAllocator Allocator, IList<uint> Indices, IList<Vector3> Vertices,
            IList<Vector3> Normals, IList<Vector4> Colors, IList<float> Extradata)
        {
            if (Colors.Count == 0) return;
            var newIndices = new NativeList<uint>(Allocator);
            var newVertices = new NativeList<Vector3>(Allocator);
            var newNormals = new NativeList<Vector3>(Allocator);
            var newColors = new NativeList<Vector4>(Allocator);
            var newExtradata = new NativeList<float>(Allocator);
            for (var i = 0; i < Indices.Count; i += 3)
            {
                var i0 = (int) Indices[i];
                var i1 = (int) Indices[i + 1];
                var i2 = (int) Indices[i + 2];

                var triangleColor = (Colors[i0] + Colors[i1] + Colors[i2]) * .33f;
                newColors.Add(triangleColor);
                newColors.Add(triangleColor);
                newColors.Add(triangleColor);

                newVertices.Add(Vertices[i0]);
                newVertices.Add(Vertices[i1]);
                newVertices.Add(Vertices[i2]);

                var normal = Vector3.Cross(Vertices[i1] - Vertices[i0], Vertices[i2] - Vertices[i0]).Normalized();
                newNormals.Add(normal);
                newNormals.Add(normal);
                newNormals.Add(normal);

                newIndices.Add((uint) newIndices.Count);
                newIndices.Add((uint) newIndices.Count);
                newIndices.Add((uint) newIndices.Count);

                if (Extradata.Count != 0)
                {
                    newExtradata.Add(Extradata[i0]);
                    newExtradata.Add(Extradata[i1]);
                    newExtradata.Add(Extradata[i2]);
                }
            }

            Indices.Clear();
            Vertices.Clear();
            Normals.Clear();
            Colors.Clear();
            Extradata.Clear();

            Indices.AddRange(newIndices);
            Vertices.AddRange(newVertices);
            Normals.AddRange(newNormals);
            Colors.AddRange(newColors);
            Extradata.AddRange(newExtradata);
            
            newIndices.Dispose();
            newVertices.Dispose();
            newNormals.Dispose();
            newColors.Dispose();
            newExtradata.Dispose();
        }

        public static void AddWindValues(IList<Vector3> Vertices, IList<Vector4> Colors, IList<float> Extradata, float Scalar = 1)
        {
            AddWindValues(Vertices, Colors, Extradata, -Vector4.One, Scalar);
        }
        
        public static void AddWindValues(IList<Vector3> Vertices, IList<Vector4> Colors, IList<float> Extradata, Vector4 ColorFilter, float Scalar = 1)
        {
            AddWindValues(
                Vertices,
                Colors,
                Extradata,
                ColorFilter,
                SupportPoint(Vertices, Colors, -Vector3.UnitY, ColorFilter),
                SupportPoint(Vertices, Colors, Vector3.UnitY, ColorFilter),
                Scalar
            );
        }

        public static void AddWindValues(IList<Vector3> Vertices, IList<Vector4> Colors, IList<float> Extradata, Vector4 ColorFilter, Vector3 Lowest, Vector3 Highest, float Scalar)
        {
            var values = new float[Vertices.Count];
            var all = ColorFilter == -Vector4.One;
            for(var i = 0; i < Extradata.Count; i++)
            {
                if(Colors[i] != ColorFilter && !all)
                {
                    values[i] = 0;
                    continue;
                }             
                var shade = Vector3.Dot(Vertices[i] - Lowest, Vector3.UnitY) / Vector3.Dot(Highest - Lowest, Vector3.UnitY);
                Extradata[i] = (shade + (float) Math.Pow(shade, 1.3)) * Scalar;
            }
        }

        public static void PaintMesh(IList<Vector4> Colors, Vector4 Color)
        {
            for(var i = 0; i < Colors.Count; i++)
            {
                Colors[i] = Color;
            }
        }

        public static void ColorMesh(IList<Vector4> Colors, Vector4 OriginalColor, Vector4 ReplacementColor)
        {
            for(var i = 0; i < Colors.Count; i++)
            {
                if((Colors[i] - OriginalColor).Length() < .01f)
                {
                    Colors[i] = ReplacementColor;
                }
            }
        }

        public static unsafe void UniqueVertices(IList<uint> Indices, IList<Vector3> Vertices, IList<Vector3> Normals,
            IList<Vector4> Colors, IList<float> Extradata)
        {
            if (Colors.Count == 0) return;

            var size = Indices.Count * sizeof(uint) + Vertices.Count * HedraSize.Vector3 +
                       Normals.Count * HedraSize.Vector3 + Extradata.Count * sizeof(float) +
                       Colors.Count * HedraSize.Vector4 + Allocator.Kilobyte * 64;
            IAllocator allocator;
            if (size <= Allocator.Megabyte * 2.5f)
            {
                var mem = stackalloc byte[size];
                allocator = new StackAllocator(size, mem);
            }
            else
            {
                allocator = new HeapAllocator(size);
            }
            
            var newIndices = new NativeList<uint>(allocator);
            var newVertices = new NativeList<Vector3>(allocator);
            var newNormals = new NativeList<Vector3>(allocator);
            var newColors = new NativeList<Vector4>(allocator);
            var newExtradata = new NativeList<float>(allocator);
            var vertexMap = new Dictionary<Vector3, int>();
            for (var i = 0; i < Indices.Count; i++)
            {
                var curr = (int) Indices[i];
                var vertex = Vertices[curr];
                var index = 0;
                if (vertexMap.ContainsKey(vertex))
                {
                    index = vertexMap[vertex];
                }
                else
                {
                    index = newVertices.Count;
                    newVertices.Add(vertex);
                    vertexMap.Add(vertex, index);

                    newColors.Add(Colors[curr]);
                    newNormals.Add(Normals[curr]);
                    if (Extradata.Count != 0)
                        newExtradata.Add(Extradata[curr]);
                }

                newIndices.Add((uint) index);
            }

            Indices.Clear();
            Vertices.Clear();
            Normals.Clear();
            Colors.Clear();
            Extradata.Clear();

            Indices.AddRange(newIndices);
            Vertices.AddRange(newVertices);
            Normals.AddRange(newNormals);
            Colors.AddRange(newColors);
            Extradata.AddRange(newExtradata);
            allocator.Dispose();
        }

        public static Vector3 SupportPoint(IList<Vector3> Vertices, IList<Vector4> Colors, Vector3 Direction)
        {
            return SupportPoint(Vertices, Colors, Direction, -Vector4.One);
        }

        public static Vector3 SupportPoint(IList<Vector3> Vertices, IList<Vector4> Colors, Vector3 Direction, Vector4 Color)
        {
            var hasColors = Colors.Count != 0;
            var highest = float.MinValue;
            var support = Vector3.Zero;
            var all = Color == -Vector4.One;
            for (var i = Vertices.Count-1; i > -1; i--)
            {
                if (hasColors)
                {
                    if (Colors[i] != Color && !all) continue;
                }

                var v = Vertices[i];
                var dot = Vector3.Dot(Direction, v);

                if (!(dot > highest)) continue;
                highest = dot;
                support = v;
            }
            return support;  
        }

        public static void GraduateColor(IList<Vector3> Vertices, IList<Vector4> Colors, Vector3 Direction)
        {
            GraduateColor(Vertices, Colors, Direction, .3f);
        }
        public static void GraduateColor(IList<Vector3> Vertices, IList<Vector4> Colors, Vector3 Direction, float Amount)
        {
            var highest = SupportPoint(Vertices, Colors, Direction);
            var lowest =  SupportPoint(Vertices, Colors, -Direction);

            var dot = Vector3.Dot(highest - lowest, Direction);
            for (var i = 0; i < Vertices.Count; i++)
            {
                var shade = Vector3.Dot(Vertices[i] - lowest, Direction) / dot;
                Colors[i] += new Vector4(Amount, Amount, Amount, 0) * shade;
            }
        }
        
        public static void Transform(IList<Vector3> Vertices, IList<Vector3> Normals, Matrix4x4 Matrix)
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], Matrix);
            }
            var normalMat = Matrix.ClearScale().ClearTranslation().Inverted().Transposed();
            for (var i = 0; i < Normals.Count; i++)
            {
                Normals[i] = Vector3.TransformNormal(Normals[i], normalMat);
            }
        }

        public static void Optimize(IAllocator Allocator, IList<uint> Indices, IList<Vector3> Vertices, IList<Vector3> Normals, IList<Vector4> Colors, IList<float> Extradata)
        {
            var hasColors = Colors.Count != 0;
            var hasExtradata = Extradata.Count != 0;
            if (!hasColors) return;
            
            var originalVertices = Vertices.Count;
            var vertices = new NativeArray<MeshOptimizerVertex>(Allocator, Vertices.Count);
            var indices = new NativeArray<uint>(Allocator, Indices.Count);
            for (var i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = new MeshOptimizerVertex
                {
                    Position = Vertices[i],
                    Normal = Normals[i],
                    Color = Colors[i],
                    Extradata = hasExtradata ? Extradata[i] : default
                };
            }
            for (var i = 0; i < indices.Length; ++i)
            {
                indices[i] = Indices[i];
            }

            Native.MeshOptimizer.Optimize(Allocator, vertices, indices, MeshOptimizerVertex.SizeInBytes);
            Indices.Clear();
            Extradata.Clear();
            Normals.Clear();
            Colors.Clear();
            Vertices.Clear();
            var len = vertices.Length;
            for (var i = 0; i < len; ++i)
            {
                var item = vertices[i];
                Normals.Add(item.Normal);
                Colors.Add(item.Color);
                Vertices.Add(item.Position);
                if(hasExtradata)
                    Extradata.Add(item.Extradata);
            }
            Indices.AddRange(indices);
            vertices.Dispose();
            indices.Dispose();
            PerformanceStatistics.RegisterMeshOptimization(Vertices.Count, originalVertices);
        }
        
        private struct MeshOptimizerVertex
        {
            public static uint SizeInBytes => sizeof(float) * 11;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Color;
            public float Extradata;
        }
    }
}