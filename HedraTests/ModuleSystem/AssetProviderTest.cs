using System.IO;
using System.Text;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using NUnit.Framework;
using OpenTK;

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
        
        [Test]
        public void TestPlyWithAlpha()
        {
            var path = $"{GameLoader.AppPath}/test.ply";
            File.WriteAllText(path, PLYWithAlpha);
            var result = _assetProvider.PLYLoader(path, Vector3.One, Vector3.Zero, Vector3.Zero);
            Assert.AreEqual(result.Vertices, new []
            {
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 2), 
                new Vector3(0, -3, 0),
            });
            
            Assert.AreEqual(result.Indices, new uint[]
            {
                0,1,2
            });

            var expectedColors = new[]
            {
                new Vector4(1, 1, 1, .5f),
                new Vector4(1, 1, 1, .5f),
                new Vector4(1, 1, 1, .5f),
            };
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    Assert.AreEqual(result.Colors[i][j], expectedColors[i][j], 0.05f);
                }
            }
            
            Assert.AreEqual(result.Normals, new []
            {
                Vector3.Zero,
                Vector3.Zero, 
                Vector3.Zero,
            });
        }
        
        private static readonly string PLYWithAlpha = 
$@"
ply
format ascii 1.0
comment Created by Blender 2.80 (sub 37) - www.blender.org, source file: ''
element vertex 3
property float x
property float y
property float z
property float nx
property float ny
property float nz
property uchar red
property uchar green
property uchar blue
property uchar alpha
element face 1
property list uchar uint vertex_indices
end_header
1.000000 0.000000 0.000000 0.000000 0.000000 0.000000 255 255 255 128
2.000000 0.000000 2.000000 0.000000 0.000000 0.000000 255 255 255 128
0.000000 -3.000000 0.000000 0.000000 0.000000 0.000000 255 255 255 128
3 0 1 2
";
    }
}