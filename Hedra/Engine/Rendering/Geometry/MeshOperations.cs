using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Hedra.Engine.Management;
using Hedra.Framework;
using Hedra.Numerics;
using Microsoft.Scripting.Utils;

namespace Hedra.Engine.Rendering.Geometry
{
    public class MeshOperations
    {
        public static unsafe void FlatMesh(IAllocator Allocator, NativeVertexData Mesh)
        {
            if (Mesh.Colors.Count == 0) return;
            var count = Mesh.Indices.Count;
            if (count == 0)
            {
                Mesh.Indices.Clear(); Mesh.Vertices.Clear(); Mesh.Normals.Clear(); Mesh.Colors.Clear(); Mesh.Extradata.Clear();
                return;
            }
            var hasExtra = Mesh.Extradata.Count != 0;
            var indices = new NativeArray<uint>(Allocator, count);
            var vertices = new NativeArray<Vector3>(Allocator, count);
            var normals = new NativeArray<Vector3>(Allocator, count);
            var colors = new NativeArray<Vector4>(Allocator, count);
            var extra = hasExtra ? new NativeArray<float>(Allocator, count) : default;
            // Acquire pointers after all allocations: a resizable arena may have moved.
            var sourceIndices = (uint*)Mesh.Indices.Pointer;
            var sourceVertices = (Vector3*)Mesh.Vertices.Pointer;
            var sourceColors = (Vector4*)Mesh.Colors.Pointer;
            var sourceExtra = hasExtra ? (float*)Mesh.Extradata.Pointer : null;
            var outputIndices = (uint*)indices.Pointer;
            var outputVertices = (Vector3*)vertices.Pointer;
            var outputNormals = (Vector3*)normals.Pointer;
            var outputColors = (Vector4*)colors.Pointer;
            var outputExtra = hasExtra ? (float*)extra.Pointer : null;
            for (var i = 0; i < count; i += 3)
            {
                var a = sourceIndices[i];
                var b = sourceIndices[i + 1];
                var c = sourceIndices[i + 2];
                var v0 = sourceVertices[a];
                var v1 = sourceVertices[b];
                var v2 = sourceVertices[c];
                var color = (sourceColors[a] + sourceColors[b] + sourceColors[c]) * .33f;
                var normal = Vector3.Cross(v1 - v0, v2 - v0).Normalized();
                outputVertices[i] = v0; outputVertices[i + 1] = v1; outputVertices[i + 2] = v2;
                outputNormals[i] = outputNormals[i + 1] = outputNormals[i + 2] = normal;
                outputColors[i] = outputColors[i + 1] = outputColors[i + 2] = color;
                outputIndices[i] = (uint)i; outputIndices[i + 1] = (uint)i + 1; outputIndices[i + 2] = (uint)i + 2;
                if (hasExtra) { outputExtra[i] = sourceExtra[a]; outputExtra[i + 1] = sourceExtra[b]; outputExtra[i + 2] = sourceExtra[c]; }
            }
            Mesh.Indices.Clear(); Mesh.Indices.AddRange(indices, count);
            Mesh.Vertices.Clear(); Mesh.Vertices.AddRange(vertices, count);
            Mesh.Normals.Clear(); Mesh.Normals.AddRange(normals, count);
            Mesh.Colors.Clear(); Mesh.Colors.AddRange(colors, count);
            Mesh.Extradata.Clear();
            if (hasExtra) Mesh.Extradata.AddRange(extra, count);
            indices.Dispose(); vertices.Dispose(); normals.Dispose(); colors.Dispose();
            if (hasExtra) extra.Dispose();
        }

        public static unsafe void UniqueVertices(NativeVertexData Mesh)
        {
            if (Mesh.Colors.Count == 0) return;
            var indexCount = Mesh.Indices.Count;
            if (indexCount == 0)
            {
                Mesh.Indices.Clear(); Mesh.Vertices.Clear(); Mesh.Normals.Clear(); Mesh.Colors.Clear(); Mesh.Extradata.Clear();
                return;
            }
            var capacity = Mesh.Vertices.Count;
            var hasExtra = Mesh.Extradata.Count != 0;
            var size = checked(indexCount * sizeof(uint) + capacity * (HedraSize.Vector3 * 2 + HedraSize.Vector4 +
                (hasExtra ? sizeof(float) : 0)) + Allocator.Kilobyte * 64);
            using var scratch = new HeapAllocator(size);
            var indices = new NativeArray<uint>(scratch, indexCount);
            var vertices = new NativeArray<Vector3>(scratch, capacity);
            var normals = new NativeArray<Vector3>(scratch, capacity);
            var colors = new NativeArray<Vector4>(scratch, capacity);
            var extra = hasExtra ? new NativeArray<float>(scratch, capacity) : default;
            // Scratch uses a separate heap. No allocation below can move the input buffers.
            var inputIndices = (uint*)Mesh.Indices.Pointer;
            var inputVertices = (Vector3*)Mesh.Vertices.Pointer;
            var inputNormals = (Vector3*)Mesh.Normals.Pointer;
            var inputColors = (Vector4*)Mesh.Colors.Pointer;
            var inputExtra = hasExtra ? (float*)Mesh.Extradata.Pointer : null;
            var outputIndices = (uint*)indices.Pointer;
            var outputVertices = (Vector3*)vertices.Pointer;
            var outputNormals = (Vector3*)normals.Pointer;
            var outputColors = (Vector4*)colors.Pointer;
            var outputExtra = hasExtra ? (float*)extra.Pointer : null;
            var map = new Dictionary<Vector3, int>();
            var count = 0;
            for (var i = 0; i < indexCount; i++)
            {
                var source = inputIndices[i];
                var position = inputVertices[source];
                ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault(map, position, out var exists);
                if (!exists)
                {
                    index = count++;
                    outputVertices[index] = position;
                    outputNormals[index] = inputNormals[source];
                    outputColors[index] = inputColors[source];
                    if (hasExtra) outputExtra[index] = inputExtra[source];
                }
                outputIndices[i] = (uint)index;
            }
            // Reacquire each destination pointer inside AddRange; its owner may grow here.
            Mesh.Indices.Clear(); Mesh.Indices.AddRange(indices, indexCount);
            Mesh.Vertices.Clear(); Mesh.Vertices.AddRange(vertices, count);
            Mesh.Normals.Clear(); Mesh.Normals.AddRange(normals, count);
            Mesh.Colors.Clear(); Mesh.Colors.AddRange(colors, count);
            Mesh.Extradata.Clear();
            if (hasExtra) Mesh.Extradata.AddRange(extra, count);
        }

        public static void FlatMesh(IList<uint> Indices, IList<Vector3> Vertices, IList<Vector3> Normals,
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

        public static void FlatMesh(IAllocator Allocator, IList<uint> Indices, IList<Vector3> Vertices,
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
                var i0 = (int)Indices[i];
                var i1 = (int)Indices[i + 1];
                var i2 = (int)Indices[i + 2];

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

                newIndices.Add((uint)newIndices.Count);
                newIndices.Add((uint)newIndices.Count);
                newIndices.Add((uint)newIndices.Count);

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

        public static void AddWindValues(IList<Vector3> Vertices, IList<Vector4> Colors, IList<float> Extradata,
            float Scalar = 1)
        {
            AddWindValues(Vertices, Colors, Extradata, -Vector4.One, Scalar);
        }

        public static void AddWindValues(IList<Vector3> Vertices, IList<Vector4> Colors, IList<float> Extradata,
            Vector4 ColorFilter, float Scalar = 1)
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

        public static void AddWindValues(IList<Vector3> Vertices, IList<Vector4> Colors, IList<float> Extradata,
            Vector4 ColorFilter, Vector3 Lowest, Vector3 Highest, float Scalar)
        {
            var values = new float[Vertices.Count];
            var all = ColorFilter == -Vector4.One;
            for (var i = 0; i < Extradata.Count; i++)
            {
                if (Colors[i] != ColorFilter && !all)
                {
                    values[i] = 0;
                    continue;
                }

                var shade = Vector3.Dot(Vertices[i] - Lowest, Vector3.UnitY) /
                            Vector3.Dot(Highest - Lowest, Vector3.UnitY);
                Extradata[i] = (shade + (float)Math.Pow(shade, 1.3)) * Scalar;
            }
        }

        public static void PaintMesh(IList<Vector4> Colors, Vector4 Color)
        {
            for (var i = 0; i < Colors.Count; i++) Colors[i] = Color;
        }

        public static void ColorMesh(IList<Vector4> Colors, Vector4 OriginalColor, Vector4 ReplacementColor)
        {
            for (var i = 0; i < Colors.Count; i++)
                if ((Colors[i] - OriginalColor).Length() < .01f)
                    Colors[i] = ReplacementColor;
        }

        public static unsafe void UniqueVertices(IList<uint> Indices, IList<Vector3> Vertices, IList<Vector3> Normals,
            IList<Vector4> Colors, IList<float> Extradata)
        {
            if (Colors.Count == 0) return;

            var size = (Indices.Count * sizeof(uint) + Vertices.Count * HedraSize.Vector3 +
                       Normals.Count * HedraSize.Vector3 + Extradata.Count * sizeof(float) +
                       Colors.Count * HedraSize.Vector4) + Allocator.Kilobyte * 64;
            IAllocator allocator;
            // Meshing also runs on worker/test threads with small stacks.
            if (size <= Allocator.Kilobyte * 128)
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
                var curr = (int)Indices[i];
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

                newIndices.Add((uint)index);
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

        public static Vector3 SupportPoint(IList<Vector3> Vertices, IList<Vector4> Colors, Vector3 Direction,
            Vector4 Color)
        {
            var hasColors = Colors.Count != 0;
            var highest = float.MinValue;
            var support = Vector3.Zero;
            var all = Color == -Vector4.One;
            for (var i = Vertices.Count - 1; i > -1; i--)
            {
                if (hasColors)
                    if (Colors[i] != Color && !all)
                        continue;

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

        public static void GraduateColor(IList<Vector3> Vertices, IList<Vector4> Colors, Vector3 Direction,
            float Amount)
        {
            var highest = SupportPoint(Vertices, Colors, Direction);
            var lowest = SupportPoint(Vertices, Colors, -Direction);

            var dot = Vector3.Dot(highest - lowest, Direction);
            for (var i = 0; i < Vertices.Count; i++)
            {
                var shade = Vector3.Dot(Vertices[i] - lowest, Direction) / dot;
                Colors[i] += new Vector4(Amount, Amount, Amount, 0) * shade;
            }
        }

        public static unsafe void Transform(NativeVertexData Mesh, Matrix4x4 Matrix)
        {
            // Transforming values cannot grow the arena. Keep stable buffer views
            // instead of resolving the allocator through IList for every access.
            var vertices = (Vector3*)Mesh.Vertices.Pointer;
            var normals = (Vector3*)Mesh.Normals.Pointer;
            for (var i = 0; i < Mesh.Vertices.Count; i++) vertices[i] = Vector3.Transform(vertices[i], Matrix);
            var normalMat = Matrix.ClearScale().ClearTranslation().Inverted().Transposed();
            for (var i = 0; i < Mesh.Normals.Count; i++) normals[i] = Vector3.TransformNormal(normals[i], normalMat);
        }

        public static void Transform(IList<Vector3> Vertices, IList<Vector3> Normals, Matrix4x4 Matrix)
        {
            for (var i = 0; i < Vertices.Count; i++) Vertices[i] = Vector3.Transform(Vertices[i], Matrix);
            var normalMat = Matrix.ClearScale().ClearTranslation().Inverted().Transposed();
            for (var i = 0; i < Normals.Count; i++) Normals[i] = Vector3.TransformNormal(Normals[i], normalMat);
        }

        public static void Optimize(IAllocator Allocator, IList<uint> Indices, IList<Vector3> Vertices,
            IList<Vector3> Normals, IList<Vector4> Colors, IList<float> Extradata)
        {
            var hasColors = Colors.Count != 0;
            var hasExtradata = Extradata.Count != 0;
            if (!hasColors) return;

            var originalVertices = Vertices.Count;
            var vertices = new NativeArray<MeshOptimizerVertex>(Allocator, Vertices.Count);
            var indices = new NativeArray<uint>(Allocator, Indices.Count);
            for (var i = 0; i < vertices.Length; ++i)
                vertices[i] = new MeshOptimizerVertex
                {
                    Position = Vertices[i],
                    Normal = Normals[i],
                    Color = Colors[i],
                    Extradata = hasExtradata ? Extradata[i] : default
                };
            for (var i = 0; i < indices.Length; ++i) indices[i] = Indices[i];

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
                if (hasExtradata)
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
