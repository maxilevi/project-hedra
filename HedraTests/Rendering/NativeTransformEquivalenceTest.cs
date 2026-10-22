using System;
using System.Numerics;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class NativeTransformEquivalenceTest
    {
        [Test]
        public void BufferTransformPreservesPositionAndNormalBits()
        {
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 8, 128);
            var mesh = new VertexData();
            var rng = new Random(43);
            for (var i = 0; i < 500; i++)
            {
                mesh.Vertices.Add(new Vector3((float)rng.NextDouble() * 1000, (float)rng.NextDouble(), (float)rng.NextDouble() * -1000));
                mesh.Normals.Add(new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble()));
            }
            mesh.Vertices[0] = new Vector3(BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)), 0, 0);
            mesh.Normals[0] = mesh.Vertices[0];
            foreach (var matrix in new[] {
                Matrix4x4.Identity, Matrix4x4.CreateTranslation(0, 0, 0), Matrix4x4.CreateTranslation(4992, 0, -4096),
                Matrix4x4.CreateRotationX(.83f) * Matrix4x4.CreateRotationY(-1.17f),
                Matrix4x4.CreateScale(1.1f, .5f, -3f) * Matrix4x4.CreateTranslation(-5000, 10000, 70)
            })
            {
                var reference = mesh.NativeClone(allocator);
                var actual = mesh.NativeClone(allocator);
                MeshOperations.Transform(reference.Vertices, reference.Normals, matrix);
                actual.Transform(matrix);
                for (var i = 0; i < mesh.Vertices.Count; i++)
                {
                    AssertBits(reference.Vertices[i], actual.Vertices[i]);
                    AssertBits(reference.Normals[i], actual.Normals[i]);
                }
                reference.Dispose(); actual.Dispose();
            }
        }

        private static void AssertBits(Vector3 Expected, Vector3 Actual)
        {
            Assert.AreEqual(BitConverter.SingleToInt32Bits(Expected.X), BitConverter.SingleToInt32Bits(Actual.X));
            Assert.AreEqual(BitConverter.SingleToInt32Bits(Expected.Y), BitConverter.SingleToInt32Bits(Actual.Y));
            Assert.AreEqual(BitConverter.SingleToInt32Bits(Expected.Z), BitConverter.SingleToInt32Bits(Actual.Z));
        }
    }
}
