using System;
using System.Linq;
using System.Numerics;
using Hedra;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using Hedra.Framework;
using NUnit.Framework;

namespace HedraTests.Rendering
{
    [NonParallelizable]
    public class TerrainSurfaceMaskTest
    {
        [TestCase(1)]
        [TestCase(2)]
        public void RowMasksMatchEightCornerClassificationIncludingTheChunkSeam(int Step)
        {
            var rng = new Random(43);
            var stride = 0u;
            for (var z = 0; z < 32; z += Step) stride |= 1u << z;
            for (var iteration = 0; iteration < 1000; iteration++)
            {
                var rows = Enumerable.Range(0, 4).Select(_ => (ulong)rng.NextInt64(0, 1L << 33)).ToArray();
                var expected = 0u;
                for (var z = 0; z < 32; z += Step)
                {
                    var positive = 0;
                    foreach (var row in rows)
                    {
                        if ((row & (1UL << z)) != 0) positive++;
                        if ((row & (1UL << (z + Step))) != 0) positive++;
                    }
                    if (positive > 0 && positive < 8) expected |= 1u << z;
                }
                Assert.AreEqual(expected, SurfaceCellMask.FromRows(rows[0], rows[1], rows[2], rows[3], Step, stride));
            }
        }

        [TestCase("flat")]
        [TestCase("hills")]
        [TestCase("caves")]
        [TestCase("water")]
        [TestCase("water_caves")]
        [TestCase("high_water")]
        [TestCase("water_missing")]
        [TestCase("tall_caves")]
        [TestCase("materials")]
        [TestCase("grass_palette")]
        public void CompleteMeshesAndCollisionMeshesMatchTheDenseTraversal(string Shape)
        {
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            using var terrain = new HeadlessTerrainFixture(Shape);
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte * 128, Allocator.Megabyte * 4);
            var dense = terrain.DenseBuilder();
            foreach (var lod in new[] { 1, 2, 4, 8 })
            {
                Utils.Rng = new Random(43);
                ChunkTerrainMeshBuilder.ClearMapping(Vector3.Zero);
                var reference = dense.CreateTerrainMesh(allocator, lod, terrain.Cache);
                var expectedNextRandom = Utils.Rng.Next();
                Utils.Rng = new Random(43);
                ChunkTerrainMeshBuilder.ClearMapping(Vector3.Zero);
                var actual = terrain.Builder.CreateTerrainMesh(allocator, lod, terrain.Cache);
                Assert.AreEqual(expectedNextRandom, Utils.Rng.Next(), "Meshing must preserve random shading draws");
                Assert.AreEqual(reference.Failed, actual.Failed);
                AssertMesh(reference.StaticData, actual.StaticData);
                AssertMesh(reference.WaterData, actual.WaterData);
                actual.Dispose();
                reference.Dispose();
            }
            var referenceCollision = dense.CreateTerrainCollisionMesh(terrain.Cache, allocator);
            var collision = terrain.Builder.CreateTerrainCollisionMesh(terrain.Cache, allocator);
            AssertMesh(referenceCollision, collision);
            collision.Dispose();
            referenceCollision.Dispose();
        }

        private static void AssertMesh(NativeVertexData Reference, NativeVertexData Actual)
        {
            CollectionAssert.AreEqual(Reference.Vertices, Actual.Vertices);
            CollectionAssert.AreEqual(Reference.Indices, Actual.Indices);
            CollectionAssert.AreEqual(Reference.Normals, Actual.Normals);
            CollectionAssert.AreEqual(Reference.Colors, Actual.Colors);
            CollectionAssert.AreEqual(Reference.Extradata, Actual.Extradata);
        }
    }
}
