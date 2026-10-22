using System.Linq;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Rendering;
using NUnit.Framework;
using System.Numerics;
using Hedra.Framework;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class ChunkMeshBorderDetectorTest
    {
        private ChunkMeshBorderDetector _meshBorderDetector;

        [SetUp]
        public void SetUp()
        {
            _meshBorderDetector = new ChunkMeshBorderDetector();
        }

        [Test]
        public void EntireBorderPreservesReferencedOrderDuplicatesAndStrictEpsilon()
        {
            using var allocator = new ResizableHeapAllocator(Allocator.Megabyte, 128);
            var mesh = new NativeVertexData(allocator);
            mesh.Vertices.AddRange(new[] {
                new Vector3(10.49f, 4, 70), new Vector3(10.5f, 4, 70),
                new Vector3(100, 4, 147.51f), new Vector3(100, 4, 147.5f),
                new Vector3(137.51f, 4, 30), new Vector3(40, 4, 20.49f), new Vector3(10, 4, 40)
            });
            mesh.Indices.AddRange(new uint[] { 1, 0, 2, 0, 3, 5, 4, 2 });
            var border = _meshBorderDetector.ProcessEntireBorder(mesh, new Vector3(10, 0, 20), new Vector3(138, 0, 148));
            CollectionAssert.AreEqual(new uint[] { 0, 2, 0, 5, 4, 2 }, border);
            mesh.Dispose();
        }
        
        [Test]
        public void TestDetectorReturnTheCorrectVertices()
        {
            var input = new[]
            {
                new Vector3(2, 5, 13.5f),
                new Vector3(3, 5, 4),
                new Vector3(0, 53, 11),
                new Vector3(0, 5, 16),
                new Vector3(8, 5, 0),
                new Vector3(16, 0, 16),
                new Vector3(0, 5, 0),
                new Vector3(3, 9, 8),
                new Vector3(34, 5, 8),
                new Vector3(7, 2, 8),
                new Vector3(16, 5, 0),
                new Vector3(2, 4, 8),
                new Vector3(24, 6, 8),
                new Vector3(13, 7, 0),
                new Vector3(16, 1, 14),
                new Vector3(3, 1, 16),
            };
            var output = _meshBorderDetector.Process(new VertexData
            {
                Vertices = input.ToList()
            }, Vector3.One * 16);
            Assert.AreEqual(new MeshBorderOutput
            {
                FrontBorder = new []
                {
                    new Vector3(3,1,16)
                },
                BackBorder = new []
                {
                    new Vector3(8,5,0),
                    new Vector3(13,7,0) 
                },
                RightBorder = new []
                {
                    new Vector3(16,1,14)
                },
                LeftBorder = new []
                {
                    new Vector3(0,53,11)
                },
                BackLeftCorner = new []
                {
                    new Vector3(0,5,0)
                },
                FrontRightCorner = new []
                {
                    new Vector3(16,0,16)
                },
                FrontLeftCorner = new []
                {
                    new Vector3(0, 5, 16)
                },
                BackRightCorner = new []
                {
                    new Vector3(16, 5, 0)
                }
            }, output);
        }
    }
}
