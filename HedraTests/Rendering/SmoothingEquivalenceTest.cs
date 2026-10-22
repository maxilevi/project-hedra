using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class SmoothingEquivalenceTest
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(6)]
        public void SmoothingPreservesWeightingDuplicatesDegeneratesAndIgnoredVertices(int Steps)
        {
            var rng = new Random(43);
            var mesh = new VertexData();
            for (var i = 0; i < 80; i++)
            {
                mesh.Vertices.Add(new Vector3(rng.Next(0, 5), rng.Next(0, 5), rng.Next(0, 5)));
                mesh.Normals.Add(new Vector3(i * .1f, i * .2f, i * .3f));
            }
            // Reference every vertex, add exact duplicate triangles, and retain degenerate faces.
            for (var i = 0; i < 80; i++) mesh.Indices.AddRange(new[] { (uint)i, (uint)((i + 1) % 80), (uint)((i + 2) % 80) });
            for (var i = 0; i < 400; i++)
            {
                var triangle = new uint[] { (uint)rng.Next(80), (uint)rng.Next(80), (uint)rng.Next(80) };
                mesh.Indices.AddRange(triangle);
                if (i % 7 == 0) mesh.Indices.AddRange(triangle);
            }
            var expected = mesh.Clone();
            var actual = mesh.Clone();
            var ignore = new HashSet<uint> { 0, 7, 43 };
            Reference(expected, ignore, Steps);
            MeshAnalyzer.ApplySmoothing(actual.Indices, actual.Vertices, actual.Colors, actual.Normals, ignore, Steps);
            CollectionAssert.AreEqual(expected.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(expected.Normals, actual.Normals);
            CollectionAssert.AreEqual(expected.Indices, actual.Indices);
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 4, 128);
            foreach (var smoothNormals in new[] { false, true })
            {
                var native = mesh.NativeClone(allocator);
                MeshAnalyzer.ApplySmoothing(native, ignore, Steps, smoothNormals);
                CollectionAssert.AreEqual(expected.Vertices, native.Vertices);
                CollectionAssert.AreEqual(smoothNormals ? expected.Normals : mesh.Normals, native.Normals);
                CollectionAssert.AreEqual(expected.Indices, native.Indices);
                native.Dispose();
            }
            var positionsOnly = mesh.Clone();
            MeshAnalyzer.ApplySmoothing(positionsOnly.Indices, positionsOnly.Vertices, positionsOnly.Colors,
                positionsOnly.Normals, ignore, Steps, false);
            CollectionAssert.AreEqual(expected.Vertices, positionsOnly.Vertices);
            CollectionAssert.AreEqual(mesh.Normals, positionsOnly.Normals);
        }

        private static void Reference(VertexData Mesh, HashSet<uint> Ignore, int Steps)
        {
            var adjacency = Enumerable.Range(0, Mesh.Vertices.Count).Select(_ => new HashSet<(uint A, uint B, uint C)>()).ToArray();
            for (var i = 0; i < Mesh.Indices.Count; i += 3)
            {
                var triangle = (A: Mesh.Indices[i], B: Mesh.Indices[i + 1], C: Mesh.Indices[i + 2]);
                adjacency[triangle.A].Add(triangle);
                adjacency[triangle.B].Add(triangle);
                adjacency[triangle.C].Add(triangle);
            }
            for (var step = 0; step < Steps; step++)
            {
                var remaps = new Dictionary<Vector3, (Vector3 Position, Vector3 Normal, int Count)>();
                for (var i = 0; i < Mesh.Vertices.Count; i++)
                {
                    var vertex = Mesh.Vertices[i];
                    if (!remaps.ContainsKey(vertex)) remaps[vertex] = default;
                    foreach (var triangle in adjacency[i].ToArray())
                    {
                        var neighbours = triangle.A == i ? new[] { triangle.B, triangle.C }
                            : triangle.B == i ? new[] { triangle.A, triangle.C } : new[] { triangle.B, triangle.C };
                        foreach (var neighbour in neighbours)
                        {
                            var remap = remaps[vertex];
                            remaps[vertex] = (remap.Position + Mesh.Vertices[(int)neighbour],
                                remap.Normal + Mesh.Normals[(int)neighbour], remap.Count + 1);
                        }
                    }
                }
                for (var i = 0; i < Mesh.Vertices.Count; i++)
                {
                    var remap = remaps[Mesh.Vertices[i]];
                    Mesh.Normals[i] = remap.Normal / remap.Count;
                    if (!Ignore.Contains((uint)i)) Mesh.Vertices[i] = remap.Position / remap.Count;
                }
            }
        }
    }
}
