using System;
using System.Linq;
using System.Numerics;
using Hedra;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Rendering;
using Hedra.Framework;
using Hedra.Rendering;
using Moq;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [NonParallelizable]
    public class PlantAppearanceCacheTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public void PreparedPaintPreservesLegacyColorsWindAndRandomSelections(bool Fern)
        {
            var provider = CacheManager.Provider;
            var random = Utils.Rng;
            using var allocator = new HeapAllocator(Allocator.Megabyte);
            try
            {
                var models = new[] { Model(0), Model(2) };
                var mock = new Mock<ICacheProvider>();
                mock.Setup(p => p.GetModel(It.IsAny<string>())).Returns(() => models[Utils.Rng.Next(0, models.Length)]);
                CacheManager.Provider = mock.Object;
                var region = Region();
                PlantDesign design = Fern ? new FernDesign() : new GrassDesign();
                Utils.Rng = new Random(487);
                var original = design.Model;
                var reference = original.NativeClone(allocator);
                var referenceRandom = new Random(31);
                design.Paint(reference, region, referenceRandom);
                var nextSelection = Utils.Rng.Next();
                var nextPaintRandom = referenceRandom.Next();

                Utils.Rng = new Random(487);
                Assert.AreSame(original, design.Model);
                var preparedRandom = new Random(31);
                var plan = design.PreparePaint(region, preparedRandom);
                var actual = original.NativeClone(allocator);
                plan.Apply(actual);
                CollectionAssert.AreEqual(reference.Colors, actual.Colors);
                CollectionAssert.AreEqual(reference.Extradata, actual.Extradata);
                CollectionAssert.AreEqual(reference.Vertices, actual.Vertices);
                Assert.AreEqual(nextSelection, Utils.Rng.Next());
                Assert.AreEqual(nextPaintRandom, preparedRandom.Next());
                reference.Dispose();
                actual.Dispose();
            }
            finally { CacheManager.Provider = provider; Utils.Rng = random; }
        }

        [Test]
        public void CacheSeparatesModelsAndPalettesAndKeepsOnlyPublishedCacheKeys()
        {
            using var allocator = new HeapAllocator(Allocator.Megabyte);
            var cache = new PlantAppearanceCache();
            var design = new FernDesign();
            var original = Model(0);
            var paint = new PreparedPlantPaint.Fern(Vector4.One);
            var painted = original.NativeClone(allocator);
            paint.Apply(painted);
            var instance = new InstanceData { ColorCache = new object(), ExtraDataCache = new object() };
            Assert.IsFalse(cache.TryGet(design, original, paint, allocator, out _));
            cache.Add(design, original, paint, instance, painted);
            painted.Dispose();
            Assert.IsTrue(cache.TryGet(design, original, new PreparedPlantPaint.Fern(Vector4.One), allocator, out var result));
            Assert.AreSame(instance.ColorCache, result.ColorCache);
            Assert.AreSame(instance.ExtraDataCache, result.ExtraDataCache);
            Assert.IsFalse(cache.TryGet(design, Model(0), paint, allocator, out _));
            Assert.IsFalse(cache.TryGet(design, original, new PreparedPlantPaint.Fern(Vector4.UnitX), allocator, out _));
            Assert.IsFalse(new PlantAppearanceCache().TryGet(design, original, paint, allocator, out _));
        }

        [Test]
        public void RandomizedPaintAndCustomSubclassesDoNotInheritTheCacheContract()
        {
            Assert.IsNull(new WheatDesign().PreparePaint(Region(), new Random(1)));
            Assert.IsNull(new CustomGrass().PreparePaint(Region(), new Random(1)));
        }

        private sealed class CustomGrass : GrassDesign { }
        private static Region Region() => new()
        {
            Colors = new RegionColor(42) { GrassColors = new[] { new Vector4(.2f, .5f, .3f, 1) } }
        };
        private static VertexData Model(float Offset) => new()
        {
            Vertices = new[] { new Vector3(0, Offset - .5f, 0), new Vector3(1, Offset + 1.5f, 0),
                new Vector3(0, Offset + 3, 1) }.ToList(),
            Normals = Enumerable.Repeat(Vector3.UnitY, 3).ToList(),
            Colors = Enumerable.Repeat(Vector4.One, 3).ToList(),
            Indices = new uint[] { 0, 1, 2 }.ToList()
        };
    }
}
