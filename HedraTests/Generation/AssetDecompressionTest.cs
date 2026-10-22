using System;
using System.IO;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [TestFixture]
    public class AssetDecompressionTest
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(262145)]
        public void StreamingExtractionMatchesLegacyAndLeavesStreamsOpen(int Size)
        {
            var expected = new byte[Size];
            new Random(1234).NextBytes(expected);
            var compressed = ZipManager.ZipBytes(expected);
            using var source = new MemoryStream(compressed);
            using var destination = new MemoryStream();

            ZipManager.UnZipTo(source, destination);

            CollectionAssert.AreEqual(expected, destination.ToArray());
            CollectionAssert.AreEqual(ZipManager.UnZipBytes(compressed), destination.ToArray());
            Assert.True(source.CanRead);
            Assert.True(destination.CanWrite);
        }

        [Test]
        public void InvalidArchiveFailsWithoutClosingCallerStreams()
        {
            using var source = new MemoryStream(new byte[64]);
            using var destination = new MemoryStream();

            Assert.Throws<InvalidDataException>(() => ZipManager.UnZipTo(source, destination));

            Assert.True(source.CanRead);
            Assert.True(destination.CanWrite);
        }
    }
}
