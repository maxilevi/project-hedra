using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Core;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.MeshOptimizer;
using Hedra.Rendering;
using System.Numerics;
using Microsoft.Scripting.Utils;

namespace Hedra.Engine.Native
{
    public static class MeshOptimizer
    {
        public static void Simplify(IAllocator Allocator, NativeVertexData Mesh, uint[] BlacklistedIndices, float Threshold, float ErrorMargin = 0.05f)
        {
            var targetIndexCount = (uint) (Mesh.Indices.Count * Threshold);
            var outIndices = new NativeArray<uint>(Allocator, Mesh.Indices.Count);
            var length = HedraCoreNative.meshopt_simplify(
                outIndices.Pointer,
                Mesh.Indices.Pointer,
                (UIntPtr) Mesh.Indices.Count,
                Mesh.Vertices.Pointer,
                (UIntPtr) Mesh.Vertices.Count,
                (UIntPtr) (HedraSize.Vector3),
                (UIntPtr) targetIndexCount,
                0.05f,
                BlacklistedIndices,
                (UIntPtr) BlacklistedIndices.Length
            );
            Mesh.Indices.Clear();
            Mesh.Indices.AddRange(outIndices, (int)length);
            outIndices.Dispose();
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
                (UIntPtr) (HedraSize.Vector3),
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
                (UIntPtr) (HedraSize.Vector3),
                (UIntPtr) targetIndexCount
            );
            Mesh.Indices.Clear();
            Mesh.Indices.AddRange(outIndices, (int)length);
            outIndices.Dispose();
        }

        public static void Optimize<T>(IAllocator Allocator, NativeArray<T> Vertices, NativeArray<uint> Indices, uint VertexSize) where T : unmanaged
        {
            Reindex(Allocator, Vertices, Indices, VertexSize);
            
            OptimizeCache(Indices, Vertices.Length);
            OptimizeOverdraw(Indices, Vertices, VertexSize, 1.05f);
            OptimizeVertexFetch(Indices, Vertices, VertexSize);
        }


        private static void Reindex<T>(IAllocator Allocator, NativeArray<T> Vertices, NativeArray<uint> Indices, uint VertexSize) where T : unmanaged
        {
            var remap = new NativeArray<uint>(Allocator, Vertices.Length);
            var totalVertices = HedraCoreNative.meshopt_generateVertexRemap(
                remap.Pointer,
                Indices.Pointer,
                (UIntPtr) Indices.Length,
                Vertices.Pointer,
                (UIntPtr) Vertices.Length,
                (UIntPtr) VertexSize
            );
            
            HedraCoreNative.meshopt_remapIndexBuffer(Indices.Pointer, Indices.Pointer, (UIntPtr) Indices.Length, remap.Pointer);
            HedraCoreNative.meshopt_remapVertexBuffer(Vertices.Pointer, Vertices.Pointer, (UIntPtr) Vertices.Length, (UIntPtr) VertexSize, remap.Pointer);

            Vertices.Trim((int)totalVertices);
            
            remap.Dispose();
        }

        private static void OptimizeCache(NativeArray<uint> Indices, int VertexCount)
        {
            HedraCoreNative.meshopt_optimizeVertexCache(Indices.Pointer, Indices.Pointer, (UIntPtr) Indices.Length, (UIntPtr) VertexCount);
        }

        private static void OptimizeOverdraw<T>(NativeArray<uint> Indices, NativeArray<T> Vertices, uint Stride, float Threshold) where T : unmanaged
        {
            HedraCoreNative.meshopt_optimizeOverdraw(Indices.Pointer, Indices.Pointer, (UIntPtr) Indices.Length, Vertices.Pointer, (UIntPtr) Vertices.Length, (UIntPtr) Stride, Threshold);
        }
        
        private static void OptimizeVertexFetch<T>(NativeArray<uint> Indices, NativeArray<T> Vertices, uint VertexSize) where T : unmanaged
        {
            HedraCoreNative.meshopt_optimizeVertexFetch(Vertices.Pointer, Indices.Pointer, (UIntPtr) Indices.Length, Vertices.Pointer, (UIntPtr) Vertices.Length, (UIntPtr) VertexSize);
        }
    }
}