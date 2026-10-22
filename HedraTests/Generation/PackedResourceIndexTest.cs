using System;
using System.IO;
using System.Text;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [TestFixture]
    public class PackedResourceIndexTest
    {
        private static MemoryStream Archive()
        {
            var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            var paths = new[] { "Assets/One/model.bin", "Assets/Two/model.bin", "Assets/texture.bin" };
            var pointers = new long[paths.Length];
            for (var i = 0; i < paths.Length; ++i)
            {
                writer.Write(paths[i]);
                pointers[i] = stream.Position;
                writer.Write(0L);
            }
            writer.Write("<end_header>");
            for (var i = 0; i < paths.Length; ++i)
            {
                var position = stream.Position;
                stream.Position = pointers[i];
                writer.Write(position);
                stream.Position = position;
                writer.Write(1);
                writer.Write((byte)(i + 1));
            }
            stream.Position = 0;
            return stream;
        }

        [TestCase("Assets/One/model.bin", 1)]
        [TestCase("Assets\\Two\\model.bin", 2)]
        [TestCase("$DataFile$Assets/texture.bin", 3)]
        [TestCase("Two/model.bin", 2)]
        [TestCase("model.bin", 1)]
        public void ResolvesFullAndLegacyPartialPaths(string Path, int Expected)
        {
            using var archive = Archive();
            var index = new PackedResourceIndex(archive);
            archive.Position = archive.Length;
            CollectionAssert.AreEqual(new[] { (byte)Expected }, index.Read(archive, Path));
            CollectionAssert.AreEqual(new[] { (byte)Expected }, index.Read(archive, Path));
        }

        [Test]
        public void MissingPathDoesNotFallBackToUnrelatedSameNamedAsset()
        {
            using var archive = Archive();
            var index = new PackedResourceIndex(archive);
            Assert.Null(index.Read(archive, "Assets/Three/model.bin"));
        }

        [Test]
        public void IndexDoesNotCloseItsStream()
        {
            using var archive = Archive();
            var index = new PackedResourceIndex(archive);
            index.Read(archive, "Assets/texture.bin");
            Assert.True(archive.CanRead);
        }
    }
}
