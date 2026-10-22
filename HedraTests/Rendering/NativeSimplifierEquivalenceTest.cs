using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Hedra.Engine.Game;
using Hedra.Engine.Native;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [NonParallelizable]
    public class NativeSimplifierEquivalenceTest
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate UIntPtr Simplify(IntPtr Output, IntPtr Indices, UIntPtr Count, IntPtr Positions, UIntPtr Vertices,
            UIntPtr Stride, UIntPtr TargetCount, float Error, IntPtr Locked, UIntPtr LockedCount);

        [TestCase("flat")]
        [TestCase("hills")]
        [TestCase("caves")]
        public void UpdatedNativeSimplifierExactlyMatchesTheReferenceLibrary(string Shape)
        {
            var path = Environment.GetEnvironmentVariable("HEDRA_MESH_BASELINE_NATIVE");
            if (string.IsNullOrEmpty(path)) Assert.Ignore("Provide the original native library for cross-version verification");
            var library = NativeLibrary.Load(path);
            try
            {
                GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
                var reference = Marshal.GetDelegateForFunctionPointer<Simplify>(NativeLibrary.GetExport(library, "meshopt_simplify"));
                using var terrain = new HeadlessTerrainFixture(Shape);
                using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 128, Allocator.Megabyte * 4);
                foreach (var lod in new[] { 1, 2, 4, 8 })
                {
                    var threshold = lod switch { 1 => .4f, 2 => .2f, 4 => .125f, _ => .075f };
                    var raw = terrain.RawMesh(allocator, lod);
                    var mesh = raw.StaticData;
                    mesh.UniqueVertices();
                    var border = new ChunkMeshBorderDetector().ProcessEntireBorder(mesh, Vector3.Zero, new Vector3(128, 0, 128));
                    foreach (var locked in new[] { border, Array.Empty<uint>(), border.Concat(border).Concat(new[] { uint.MaxValue }).ToArray() })
                    {
                        var expected = RunReference(reference, allocator, mesh, locked, threshold);
                        var actual = new NativeArray<uint>(allocator, mesh.Indices.Count);
                        var count = HedraCoreNative.meshopt_simplify(actual.Pointer, mesh.Indices.Pointer, (UIntPtr)mesh.Indices.Count,
                            mesh.Vertices.Pointer, (UIntPtr)mesh.Vertices.Count, (UIntPtr)12, (UIntPtr)(uint)(mesh.Indices.Count * threshold),
                            .05f, locked, (UIntPtr)locked.Length);
                        CollectionAssert.AreEqual(expected, actual.Take((int)count), $"{Shape} LOD {lod} locks {locked.Length}");
                        actual.Dispose();
                    }
                    raw.Dispose();
                }
            }
            finally { NativeLibrary.Free(library); }
        }

        [TestCase(1023)]
        [TestCase(1024)]
        [TestCase(1025)]
        [TestCase(4096)]
        public void PositionHashCapacityPreservesCanonicalVerticesAndSeams(int VertexCount)
        {
            var path = Environment.GetEnvironmentVariable("HEDRA_MESH_BASELINE_NATIVE");
            if (string.IsNullOrEmpty(path)) Assert.Ignore("Provide the original native library for cross-version verification");
            var library = NativeLibrary.Load(path);
            try
            {
                GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
                var reference = Marshal.GetDelegateForFunctionPointer<Simplify>(NativeLibrary.GetExport(library, "meshopt_simplify"));
                using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, 128);
                var mesh = new NativeVertexData(allocator);
                for (var i = 0; i < VertexCount; i++)
                    mesh.Vertices.Add(new Vector3(i % 32, MathF.Sin(i * .1f) * .15f, i / 32));
                for (var i = 0; i + 33 < VertexCount; i++)
                {
                    if (i % 32 == 31) continue;
                    mesh.Indices.AddRange(new[] { (uint)i, (uint)(i + 32), (uint)(i + 1),
                        (uint)(i + 1), (uint)(i + 32), (uint)(i + 33) });
                }
                foreach (var seams in new[] { false, true })
                {
                    if (seams)
                        for (var i = 64; i < VertexCount; i += 11) mesh.Vertices[i] = mesh.Vertices[i - 32];
                    foreach (var locked in new[] { Array.Empty<uint>(), new uint[] { 0, 0, 1, (uint)(VertexCount - 1), uint.MaxValue } })
                    {
                        var expected = RunReference(reference, allocator, mesh, locked, .4f);
                        using var actual = new NativeArray<uint>(allocator, mesh.Indices.Count);
                        var count = HedraCoreNative.meshopt_simplify(actual.Pointer, mesh.Indices.Pointer, (UIntPtr)mesh.Indices.Count,
                            mesh.Vertices.Pointer, (UIntPtr)mesh.Vertices.Count, (UIntPtr)12, (UIntPtr)(uint)(mesh.Indices.Count * .4f),
                            .05f, locked, (UIntPtr)locked.Length);
                        CollectionAssert.AreEqual(expected, actual.Take((int)count), $"vertices={VertexCount} seams={seams}");
                    }
                }
                mesh.Dispose();
            }
            finally { NativeLibrary.Free(library); }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void PlanarShortcutPreservesCollapseOrderAndFallsBackForBordersAndNonplanarData(int Axis)
        {
            var path = Environment.GetEnvironmentVariable("HEDRA_MESH_BASELINE_NATIVE");
            if (string.IsNullOrEmpty(path)) Assert.Ignore("Provide the original native library for cross-version verification");
            var library = NativeLibrary.Load(path);
            try
            {
                GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
                var reference = Marshal.GetDelegateForFunctionPointer<Simplify>(NativeLibrary.GetExport(library, "meshopt_simplify"));
                using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, 128);
                foreach (var perturb in new[] { false, true })
                foreach (var seams in new[] { false, true })
                {
                    var mesh = new NativeVertexData(allocator);
                    const int width = 17;
                    for (var i = 0; i < width * width; i++)
                    {
                        var a = i % width * .031f;
                        var b = i / width * .071f;
                        var constant = 1050f + (perturb && i == 128 ? .001f : 0);
                        mesh.Vertices.Add(Axis == 0 ? new Vector3(constant, a, b)
                            : Axis == 1 ? new Vector3(a, constant, b) : new Vector3(a, b, constant));
                    }
                    for (var i = 0; i + width + 1 < width * width; i++)
                    {
                        if (i % width == width - 1) continue;
                        mesh.Indices.AddRange(new[] { (uint)i, (uint)(i + width), (uint)(i + 1),
                            (uint)(i + 1), (uint)(i + width), (uint)(i + width + 1) });
                    }
                    if (seams)
                        for (var i = 64; i < mesh.Vertices.Count; i += 11) mesh.Vertices[i] = mesh.Vertices[i - width];
                    var border = Enumerable.Range(0, width * width)
                        .Where(i => i % width == 0 || i % width == width - 1 || i / width == 0 || i / width == width - 1)
                        .Select(i => (uint)i).ToArray();
                    foreach (var locked in new[] { border, Array.Empty<uint>() })
                    foreach (var threshold in new[] { .4f, .2f, .075f })
                    {
                        var expected = RunReference(reference, allocator, mesh, locked, threshold);
                        using var actual = new NativeArray<uint>(allocator, mesh.Indices.Count);
                        var count = HedraCoreNative.meshopt_simplify(actual.Pointer, mesh.Indices.Pointer, (UIntPtr)mesh.Indices.Count,
                            mesh.Vertices.Pointer, (UIntPtr)mesh.Vertices.Count, (UIntPtr)12, (UIntPtr)(uint)(mesh.Indices.Count * threshold),
                            .05f, locked, (UIntPtr)locked.Length);
                        CollectionAssert.AreEqual(expected, actual.Take((int)count),
                            $"axis={Axis} perturb={perturb} seams={seams} locks={locked.Length} threshold={threshold}");
                    }
                    mesh.Dispose();
                }
            }
            finally { NativeLibrary.Free(library); }
        }

        private static unsafe uint[] RunReference(Simplify Function, IAllocator Allocator, NativeVertexData Mesh, uint[] Locked, float Threshold)
        {
            var output = new NativeArray<uint>(Allocator, Mesh.Indices.Count);
            fixed (uint* locked = Locked)
            {
                var count = Function(output.Pointer, Mesh.Indices.Pointer, (UIntPtr)Mesh.Indices.Count, Mesh.Vertices.Pointer,
                    (UIntPtr)Mesh.Vertices.Count, (UIntPtr)12, (UIntPtr)(uint)(Mesh.Indices.Count * Threshold), .05f,
                    (IntPtr)locked, (UIntPtr)Locked.Length);
                var result = output.Take((int)count).ToArray();
                output.Dispose();
                return result;
            }
        }
    }
}
