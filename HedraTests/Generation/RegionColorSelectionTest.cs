using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Hedra.BiomeSystem;
using NUnit.Framework;

namespace HedraTests.Generation
{
    public class RegionColorSelectionTest
    {
        [TestCase(0)]
        [TestCase(-173)]
        [TestCase(int.MaxValue)]
        public void CachedSelectionsMatchSeededRandomAcrossPaletteChanges(int Seed)
        {
            var region = new RegionColor(Seed);
            foreach (var length in new[] { 1, 8, 3, 16, 8, 1 })
            {
                var palette = Enumerable.Range(0, length).Select(i => new Vector4(i, length, Seed, 1)).ToArray();
                region.GrassColors = palette;
                region.WoodColors = palette;
                region.LeavesColors = palette;
                region.StonePathColors = palette;
                Assert.AreEqual(palette[new Random(unchecked(Seed + 54)).Next(0, length)], region.GrassColor);
                Assert.AreEqual(palette[new Random(unchecked(Seed + 12)).Next(0, length)], region.WoodColor);
                Assert.AreEqual(palette[new Random(unchecked(Seed + 42)).Next(0, length)], region.LeavesColor);
                Assert.AreEqual(palette[new Random(unchecked(Seed + 54)).Next(0, length)], region.StonePathColor);
                // Replacing values at the same length must not return an old cached color.
                region.GrassColors = palette.Select(c => c * 2).ToArray();
                Assert.AreEqual(palette[new Random(unchecked(Seed + 54)).Next(0, length)] * 2, region.GrassColor);
            }
            region.StonePathColors = null;
            Assert.DoesNotThrow(() => _ = region.StonePathColor);
        }

        [Test]
        public void ConcurrentFirstReadsPreserveTheSameSeededChoice()
        {
            var palette = Enumerable.Range(0, 13).Select(i => new Vector4(i, 0, 0, 1)).ToArray();
            var region = new RegionColor(37) { GrassColors = palette };
            var expected = palette[new Random(37 + 54).Next(0, palette.Length)];
            Parallel.For(0, 1000, _ => Assert.AreEqual(expected, region.GrassColor));
        }
    }
}
