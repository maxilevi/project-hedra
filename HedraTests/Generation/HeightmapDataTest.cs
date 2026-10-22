using System.IO;
using Hedra;
using Hedra.Engine.Generation;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [TestFixture]
    public class HeightmapDataTest
    {
        [Test]
        public void CurrentFormatRoundTripsWithoutBinaryFormatter()
        {
            var original = new HeightmapData
            {
                Heightmap = new[] { new[] { -1.5f, 2f }, new[] { 3f, 4.5f } },
                HeightmapType = new[]
                {
                    new[] { BlockType.Air, BlockType.Air },
                    new[] { BlockType.Air, BlockType.Air }
                }
            };
            var result = new HeightmapData().FromByteArray(original.ToByteArray());
            Assert.AreEqual(original.Heightmap, result.Heightmap);
            Assert.AreEqual(original.HeightmapType, result.HeightmapType);
        }

        [Test]
        public void LegacyFormatReportsAnActionableError()
        {
            var error = Assert.Throws<InvalidDataException>(
                () => new HeightmapData().FromByteArray(System.BitConverter.GetBytes(1.2f)));
            StringAssert.Contains("Re-export", error.Message);
        }
    }
}
