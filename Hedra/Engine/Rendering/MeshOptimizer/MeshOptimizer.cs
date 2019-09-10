using System;
using System.Linq;
using System.Runtime.InteropServices;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering.MeshOptimizer
{
    public static class MeshOptimizer
    {
        private const string MeshOptimizerDLL = "meshoptimizer32.dll";
        
        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint meshopt_simplify(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount, float TargetError, uint[] Blacklisted, UIntPtr BlacklistedIndexCount);

        [DllImport(MeshOptimizerDLL, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint meshopt_simplifySloppy(uint[] Destination, uint[] Indices, UIntPtr IndexCount, IntPtr Vertices, UIntPtr VertexCount, IntPtr BlacklistedVertices, UIntPtr BlacklistLength, UIntPtr VertexPositionsStride, UIntPtr TargetIndexCount);

        public static VertexData Simplify(VertexData Mesh, uint[] BlacklistedIndices, float Threshold)
        {
            var indices = Mesh.Indices.ToArray();
            var vertices = Mesh.Vertices.ToArray();
            var targetIndexCount = (uint)(indices.Length * Threshold);
            var outIndices = new uint[indices.Length];
            var verticesPointer = Pointer.Create(vertices);
            var length = meshopt_simplify(
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
            return Mesh;
        }
        
        public static VertexData SimplifySloppy(VertexData Mesh, Vector3[] Border, float Threshold)
        {
            var indices = Mesh.Indices.ToArray();
            var vertices = Mesh.Vertices.ToArray();
            var targetIndexCount = (uint)(indices.Length * Threshold);
            var outIndices = new uint[indices.Length];
            var verticesPointer = Pointer.Create(vertices);
            var borderPointer = Pointer.Create(Border);
            var length = meshopt_simplifySloppy(
                outIndices,
                indices,
                (UIntPtr) indices.Length,
                verticesPointer.Address,
                (UIntPtr) vertices.Length,
                borderPointer.Address,
                (UIntPtr) Border.Length,
                (UIntPtr) (Vector3.SizeInBytes),
                (UIntPtr) targetIndexCount
            );
            borderPointer.Free();
            verticesPointer.Free();
            Mesh.Indices = outIndices.Take((int)length).ToList();
            return Mesh;
        }
    }
}