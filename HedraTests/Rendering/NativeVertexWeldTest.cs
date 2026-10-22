using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class NativeVertexWeldTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public void NativeWeldPreservesFirstReferencedAttributesAndIndexOrder(bool Extra)
        {
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 8, 256);
            var rng = new Random(43);
            var model = new VertexData();
            for (var i = 0; i < 3000; i++)
            {
                model.Vertices.Add(new Vector3(rng.Next(0, 8), rng.Next(0, 8), rng.Next(0, 8)));
                model.Normals.Add(new Vector3(i, i + 1, i + 2));
                model.Colors.Add(new Vector4(i % 7, i % 13, i % 17, 1));
                if (Extra) model.Extradata.Add(i * .25f);
            }
            model.Vertices[1] = new Vector3(BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)), 0, 0);
            model.Vertices[2] = Vector3.Zero;
            model.Indices.AddRange(Enumerable.Range(0, 6000).Select(_ => (uint)rng.Next(0, model.Vertices.Count)));
            var expected = model.NativeClone(allocator);
            var actual = model.NativeClone(allocator);
            MeshOperations.UniqueVertices(expected.Indices, expected.Vertices, expected.Normals, expected.Colors, expected.Extradata);
            actual.UniqueVertices();
            CollectionAssert.AreEqual(expected.Indices, actual.Indices);
            CollectionAssert.AreEqual(expected.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(expected.Normals, actual.Normals);
            CollectionAssert.AreEqual(expected.Colors, actual.Colors);
            CollectionAssert.AreEqual(expected.Extradata, actual.Extradata);
            expected.Dispose();
            actual.Dispose();
        }
    }
}
