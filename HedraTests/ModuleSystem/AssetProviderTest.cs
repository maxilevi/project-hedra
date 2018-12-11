using System.IO;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using NUnit.Framework;

namespace HedraTests.ModuleSystem
{
    [TestFixture]
    public class AssetProviderTest
    {
        private CompressedAssetProvider _assetProvider;

        [SetUp]
        public void Setup()
        {
            _assetProvider = new CompressedAssetProvider();
        }

        [Test]
        public void TestGameFolderPrefix()
        {
            var path = $"{GameLoader.AppPath}/test_file.txt";
            var expected = new byte[]
            {
                0, 1, 2, 3, 4, 5
            };
            File.WriteAllBytes(path, expected);
            Assert.AreEqual(_assetProvider.ReadPath("$GameFolder$/test_file.txt"), expected);
        }
    }
}