using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Framework;
using Hedra.Rendering;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    public class NativeFlattenEquivalenceTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public void FlatMeshPreservesGeometryAveragedColorsNormalsAndExtrasAcrossArenaGrowth(bool Extra)
        {
            var rng = new Random(43);
            var mesh = new VertexData();
            for (var i = 0; i < 400; i++)
            {
                mesh.Vertices.Add(new Vector3(rng.Next(0, 9), rng.Next(0, 9), rng.Next(0, 9)));
                mesh.Normals.Add(Vector3.UnitY);
                mesh.Colors.Add(new Vector4(i * .01f, i * .02f, i * .03f, 1));
                if (Extra) mesh.Extradata.Add(i * .001f);
            }
            mesh.Indices.AddRange(Enumerable.Range(0, 6000).Select(_ => (uint)rng.Next(mesh.Vertices.Count)));
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 8, 128);
            var reference = mesh.NativeClone(allocator);
            var actual = mesh.NativeClone(allocator);
            MeshOperations.FlatMesh(allocator, reference.Indices, reference.Vertices, reference.Normals, reference.Colors, reference.Extradata);
            actual.Flat(allocator);
            CollectionAssert.AreEqual(reference.Indices, actual.Indices);
            CollectionAssert.AreEqual(reference.Vertices, actual.Vertices);
            CollectionAssert.AreEqual(reference.Normals, actual.Normals);
            CollectionAssert.AreEqual(reference.Colors, actual.Colors);
            CollectionAssert.AreEqual(reference.Extradata, actual.Extradata);
            reference.Dispose(); actual.Dispose();
        }
    }
}
