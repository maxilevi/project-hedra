using System;
using Hedra.Engine.Rendering;
using Hedra.Framework;

namespace Hedra.Engine.Native
{
    public static class MeshOptimizer
    {
        public static void Simplify(IAllocator Allocator, NativeVertexData Mesh, uint[] BlacklistedIndices,
            float Threshold, float ErrorMargin = 0.05f)
        {
            if (Mesh.Indices.Count == 0) return;
            var targetIndexCount = (uint)(Mesh.Indices.Count * Threshold);
            var outIndices = new NativeArray<uint>(Allocator, Mesh.Indices.Count);
            var length = HedraCoreNative.meshopt_simplify(
                outIndices.Pointer,
                Mesh.Indices.Pointer,
                (UIntPtr)Mesh.Indices.Count,
                Mesh.Vertices.Pointer,
                (UIntPtr)Mesh.Vertices.Count,
                (UIntPtr)HedraSize.Vector3,
                (UIntPtr)targetIndexCount,
                0.05f,
                BlacklistedIndices,
                (UIntPtr)BlacklistedIndices.Length
            );
            Mesh.Indices.Clear();
            Mesh.Indices.AddRange(outIndices, (int)length);
            outIndices.Dispose();
        }

        public static void SimplifySloppy(IAllocator Allocator, NativeVertexData Mesh, float Threshold)
        {
            if (Mesh.Indices.Count == 0) return;
            var targetIndexCount = (uint)(Mesh.Indices.Count * Threshold);
            var outIndices = new NativeArray<uint>(Allocator, Mesh.Indices.Count);
            var length = HedraCoreNative.meshopt_simplifySloppy(
                outIndices.Pointer,
                Mesh.Indices.Pointer,
                (UIntPtr)Mesh.Indices.Count,
                Mesh.Vertices.Pointer,
                (UIntPtr)Mesh.Vertices.Count,
                (UIntPtr)HedraSize.Vector3,
                (UIntPtr)targetIndexCount
            );
            Mesh.Indices.Clear();
            Mesh.Indices.AddRange(outIndices, (int)length);
            outIndices.Dispose();
        }

        public static void Optimize<T>(IAllocator Allocator, NativeArray<T> Vertices, NativeArray<uint> Indices,
            uint VertexSize) where T : unmanaged
        {
            Reindex(Allocator, Vertices, Indices, VertexSize);

            OptimizeCache(Indices, Vertices.Length);
            OptimizeOverdraw(Indices, Vertices, VertexSize, 1.05f);
            OptimizeVertexFetch(Indices, Vertices, VertexSize);
        }


        private static void Reindex<T>(IAllocator Allocator, NativeArray<T> Vertices, NativeArray<uint> Indices,
            uint VertexSize) where T : unmanaged
        {
            var remap = new NativeArray<uint>(Allocator, Vertices.Length);
            var totalVertices = HedraCoreNative.meshopt_generateVertexRemap(
                remap.Pointer,
                Indices.Pointer,
                (UIntPtr)Indices.Length,
                Vertices.Pointer,
                (UIntPtr)Vertices.Length,
                (UIntPtr)VertexSize
            );

            HedraCoreNative.meshopt_remapIndexBuffer(Indices.Pointer, Indices.Pointer, (UIntPtr)Indices.Length,
                remap.Pointer);
            HedraCoreNative.meshopt_remapVertexBuffer(Vertices.Pointer, Vertices.Pointer, (UIntPtr)Vertices.Length,
                (UIntPtr)VertexSize, remap.Pointer);

            Vertices.Trim((int)totalVertices);

            remap.Dispose();
        }

        private static void OptimizeCache(NativeArray<uint> Indices, int VertexCount)
        {
            HedraCoreNative.meshopt_optimizeVertexCache(Indices.Pointer, Indices.Pointer, (UIntPtr)Indices.Length,
                (UIntPtr)VertexCount);
        }

        private static void OptimizeOverdraw<T>(NativeArray<uint> Indices, NativeArray<T> Vertices, uint Stride,
            float Threshold) where T : unmanaged
        {
            HedraCoreNative.meshopt_optimizeOverdraw(Indices.Pointer, Indices.Pointer, (UIntPtr)Indices.Length,
                Vertices.Pointer, (UIntPtr)Vertices.Length, (UIntPtr)Stride, Threshold);
        }

        private static void OptimizeVertexFetch<T>(NativeArray<uint> Indices, NativeArray<T> Vertices, uint VertexSize)
            where T : unmanaged
        {
            HedraCoreNative.meshopt_optimizeVertexFetch(Vertices.Pointer, Indices.Pointer, (UIntPtr)Indices.Length,
                Vertices.Pointer, (UIntPtr)Vertices.Length, (UIntPtr)VertexSize);
        }
    }
}