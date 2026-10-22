using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Rendering;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class IndexedMarchingCubesTest
    {
        [Test]
        public void RollingEdgeCachePreservesBitsAcrossLayersDirectionsAndZeroDensityCorners()
        {
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, 128);
            var reference = new NativeVertexData(allocator);
            var actual = new NativeVertexData(allocator);
            var map = new Dictionary<Vector3, int>();
            var triangles = MarchingCubes.NewTriangleBuffer();
            var vertices = MarchingCubes.NewVertexBuffer();
            using var buffer = new MarchingCubes.IndexedCellBuffer(1, 4);
            var rng = new Random(53);
            var density = new float[5, 6, 5];
            for (var x = 0; x < 5; x++)
            for (var y = 0; y < 6; y++)
            for (var z = 0; z < 5; z++)
                density[x, y, z] = rng.Next(8) == 0 ? 0 : (float)((rng.NextDouble() - .5) * Math.Pow(10, rng.Next(-5, 5)));
            var corners = new[] { (0,0,0), (1,0,0), (1,0,1), (0,0,1), (0,1,0), (1,1,0), (1,1,1), (0,1,1) };
            var cell = new GridCell { P = new Vector3[8], Density = new float[8] };
            var water = false;
            var river = false;
            // Include skipped layers, reversed traversal, and a repeated layer.
            foreach (var x in new[] { 0, 1, 3, 2, 0 })
            for (var y = 1; y <= 4; y++)
            for (var z = 0; z < 4; z++)
            {
                for (var i = 0; i < 8; i++)
                {
                    var (dx, dy, dz) = corners[i];
                    cell.P[i] = new Vector3(x + dx, y + dy, z + dz) * 4;
                    cell.Density[i] = density[x + dx, y + dy, z + dz];
                }
                var color = new Vector4(x * .1f, y * .1f, z * .1f, 1);
                MarchingCubes.Polygonise(ref cell, ref vertices, ref triangles, out var count);
                MarchingCubes.Build(ref reference, ref color, ref triangles, ref count, ref water, ref river);
                buffer.PrepareCell(x, y, z);
                MarchingCubes.PolygoniseIndexedForRendering(ref cell, vertices, actual, color, water, river, map, buffer);
            }
            reference.UniqueVertices();
            Assert.AreEqual(reference.Vertices.Count, actual.Vertices.Count);
            for (var i = 0; i < reference.Vertices.Count; i++)
            {
                Assert.AreEqual(BitConverter.SingleToInt32Bits(reference.Vertices[i].X), BitConverter.SingleToInt32Bits(actual.Vertices[i].X));
                Assert.AreEqual(BitConverter.SingleToInt32Bits(reference.Vertices[i].Y), BitConverter.SingleToInt32Bits(actual.Vertices[i].Y));
                Assert.AreEqual(BitConverter.SingleToInt32Bits(reference.Vertices[i].Z), BitConverter.SingleToInt32Bits(actual.Vertices[i].Z));
            }
            CollectionAssert.AreEqual(reference.Indices, actual.Indices);
            CollectionAssert.AreEqual(reference.Colors, actual.Colors);
            reference.Dispose(); actual.Dispose();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DirectEmissionMatchesEveryCubeConfiguration(bool Water, bool RiverConstant)
        {
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 16, 128);
            var reference = new NativeVertexData(allocator);
            var actual = new NativeVertexData(allocator);
            var map = new Dictionary<Vector3, int>();
            var triangles = MarchingCubes.NewTriangleBuffer();
            var vertices = MarchingCubes.NewVertexBuffer();
            var rng = new Random(43);
            var indexedBuffer = new MarchingCubes.IndexedCellBuffer();
            var cell = new GridCell { P = new Vector3[8], Density = new float[8] };
            var corners = new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1),
                new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(0,1,1) };
            for (var configuration = 0; configuration < 256; configuration++)
            for (var height = 0; height < 4; height++)
            {
                var color = new Vector4(configuration * .003f, height * .2f, .7f, 1);
                for (var corner = 0; corner < 8; corner++)
                {
                    cell.P[corner] = corners[corner] * 4 + new Vector3(configuration % 4, height * 40, 0);
                    cell.Density[corner] = (configuration & (1 << corner)) != 0
                        ? .01f + (float)rng.NextDouble() * 20
                        : corner % 3 == 0 ? 0 : -.01f - (float)rng.NextDouble() * 20;
                }
                MarchingCubes.Polygonise(ref cell, ref vertices, ref triangles, out var count);
                MarchingCubes.Build(ref reference, ref color, ref triangles, ref count, ref Water, ref RiverConstant);
                MarchingCubes.PolygoniseIndexedForRendering(ref cell, vertices, actual, color, Water, RiverConstant, map, indexedBuffer);
            }
            reference.UniqueVertices();
            CollectionAssert.AreEqual(reference.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(reference.Colors, actual.Colors);
            CollectionAssert.AreEqual(reference.Indices, actual.Indices);
            reference.Flat(allocator);
            actual.Flat(allocator);
            CollectionAssert.AreEqual(reference.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(reference.Colors, actual.Colors);
            CollectionAssert.AreEqual(reference.Normals, actual.Normals);
            reference.Dispose(); actual.Dispose();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FusedEmissionPreservesFirstColorsWindingAndClipping(bool Water)
        {
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 4, 128);
            var reference = new NativeVertexData(allocator);
            var actual = new NativeVertexData(allocator);
            var map = new Dictionary<Vector3, int>();
            var triangles = MarchingCubes.NewTriangleBuffer();
            var isRiverConstant = true;
            for (var cell = 0; cell < 25; cell++)
            {
                var color = new Vector4(cell * .03f, cell * .02f, cell * .01f, 1);
                for (var i = 0; i < 5; i++)
                {
                    var y = cell % 3 == 0 ? 0 : 120;
                    triangles[i].Vertices[0] = new Vector3(0, y, i);
                    triangles[i].Vertices[1] = new Vector3(i, y + 1, 0);
                    triangles[i].Vertices[2] = new Vector3(i + 1, y + 2, 1);
                }
                var count = 5;
                MarchingCubes.Build(ref reference, ref color, ref triangles, ref count, ref Water, ref isRiverConstant);
                MarchingCubes.BuildIndexedForRendering(actual, color, triangles, count, Water, isRiverConstant, map);
            }
            reference.UniqueVertices();
            CollectionAssert.AreEqual(reference.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(reference.Colors, actual.Colors);
            CollectionAssert.AreEqual(reference.Indices, actual.Indices);
            reference.Flat(allocator);
            actual.Flat(allocator);
            CollectionAssert.AreEqual(reference.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(reference.Colors, actual.Colors);
            CollectionAssert.AreEqual(reference.Normals, actual.Normals);
            CollectionAssert.AreEqual(reference.Indices, actual.Indices);
            reference.Dispose(); actual.Dispose();
        }
    }
}
