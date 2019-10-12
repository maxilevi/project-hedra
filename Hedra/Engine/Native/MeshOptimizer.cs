using System;
using System.Linq;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.MeshOptimizer;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Native
{
    public static class MeshOptimizer
    {
        
        public static void Simplify(VertexData Mesh, float Threshold)
        {
            Simplify(Mesh, new uint[0], Threshold);
        }

        public static void Simplify(VertexData Mesh, uint[] BlacklistedIndices, float Threshold)
        {
            Simplify(Mesh, BlacklistedIndices, Threshold, 0.05f);
        }
        
        public static void Simplify(VertexData Mesh, uint[] BlacklistedIndices, float Threshold, float ErrorMargin)
        {
            var indices = Mesh.Indices.ToArray();
            var vertices = Mesh.Vertices.ToArray();
            var targetIndexCount = (uint) (indices.Length * Threshold);//Math.Max(384, (uint)(indices.Length * Threshold));
            var outIndices = new uint[indices.Length];
            var verticesPointer = Pointer.Create(vertices);
            var length = HedraCoreNative.meshopt_simplify(
                outIndices,
                indices,
                (UIntPtr) indices.Length,
                verticesPointer.Address,
                (UIntPtr) vertices.Length,
                (UIntPtr) (Vector3.SizeInBytes),
                (UIntPtr) targetIndexCount,
                .05f,
                BlacklistedIndices,
                (UIntPtr) BlacklistedIndices.Length
            );
            verticesPointer.Free();
            Mesh.Indices = outIndices.Take((int)length).ToList();
        }
        
        public static void SimplifySloppy(VertexData Mesh, float Threshold)
        {
            var indices = Mesh.Indices.ToArray();
            var vertices = Mesh.Vertices.ToArray();
            var targetIndexCount = (uint)(indices.Length * Threshold);
            var outIndices = new uint[indices.Length];
            var outIndicesPointer = Pointer.Create(outIndices);
            var indicesPointer = Pointer.Create(indices);
            var verticesPointer = Pointer.Create(vertices);
            var length = HedraCoreNative.meshopt_simplifySloppy(
                outIndicesPointer.Address,
                indicesPointer.Address,
                (UIntPtr) indices.Length,
                verticesPointer.Address,
                (UIntPtr) vertices.Length,
                (UIntPtr) (Vector3.SizeInBytes),
                (UIntPtr) targetIndexCount
            );
            outIndicesPointer.Free();
            indicesPointer.Free();
            verticesPointer.Free();
            Mesh.Indices = outIndices.Take((int)length).ToList();
        }
        
        public static void SimplifySloppy(IAllocator Allocator, NativeVertexData Mesh, float Threshold)
        {
            var targetIndexCount = (uint)(Mesh.Indices.Count * Threshold);
            var outIndices = new NativeArray<uint>(Allocator, Mesh.Indices.Count);
            var length = HedraCoreNative.meshopt_simplifySloppy(
                outIndices.Pointer,
                Mesh.Indices.Pointer,
                (UIntPtr) Mesh.Indices.Count,
                Mesh.Vertices.Pointer,
                (UIntPtr) Mesh.Vertices.Count,
                (UIntPtr) (Vector3.SizeInBytes),
                (UIntPtr) targetIndexCount
            );
            Mesh.Indices.Clear();
            Mesh.Indices.AddRange(outIndices, (int)length);
        }

        public static Tuple<T[], uint[]> Optimize<T>(T[] Vertices, uint[] Indices, uint VertexSize)
        {
            var results = Reindex(Vertices, Indices, VertexSize);
            var vertices = results.Item1;
            var indices = results.Item2;

            OptimizeCache(indices, vertices.Length);
            OptimizeOverdraw(indices, vertices, VertexSize, 1.05f);
            OptimizeVertexFetch(indices, vertices, VertexSize);
            return Tuple.Create(vertices, indices);
        }


        public static Tuple<T[], uint[]> Reindex<T>(T[] Vertices, uint[] Indices, uint VertexSize)
        {
            var remap = new uint[Vertices.Length];
            var vertexPointer = Pointer.Create(Vertices);
            var indexCount = (Indices?.Length ?? Vertices.Length);
            var totalVertices = HedraCoreNative.meshopt_generateVertexRemap(
                remap,
                Indices,
                (UIntPtr) indexCount,
                vertexPointer.Address,
                (UIntPtr) Vertices.Length,
                (UIntPtr) VertexSize
            );

            var indices = new uint[indexCount];
            HedraCoreNative.meshopt_remapIndexBuffer(indices, Indices, (UIntPtr) indexCount, remap);

            var vertices = new T[totalVertices];
            var targetVerticesPointer = Pointer.Create(vertices);
            HedraCoreNative.meshopt_remapVertexBuffer(targetVerticesPointer.Address, vertexPointer.Address, (UIntPtr) Vertices.Length, (UIntPtr) VertexSize, remap);

            vertexPointer.Free();
            targetVerticesPointer.Free();
            return Tuple.Create(vertices, indices);
        }

        public static void OptimizeCache(uint[] Indices, int VertexCount)
        {
            HedraCoreNative.meshopt_optimizeVertexCache(Indices, Indices, (UIntPtr) Indices.Length, (UIntPtr) VertexCount);
        }
        
        public static void OptimizeOverdraw<T>(uint[] Indices, T[] Vertices, uint Stride, float Threshold)
        {
            var pointer = Pointer.Create(Vertices);
            HedraCoreNative.meshopt_optimizeOverdraw(Indices, Indices, (UIntPtr) Indices.Length, pointer.Address, (UIntPtr) Vertices.Length, (UIntPtr) Stride, Threshold);
            pointer.Free();
        }
        
        public static void OptimizeVertexFetch<T>(uint[] Indices, T[] Vertices, uint VertexSize)
        {
            var pointer = Pointer.Create(Vertices);
            HedraCoreNative.meshopt_optimizeVertexFetch(pointer.Address, Indices, (UIntPtr) Indices.Length, pointer.Address, (UIntPtr) Vertices.Length, (UIntPtr) VertexSize);
            pointer.Free();
        }
    }
}