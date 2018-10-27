using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Generation
{
    [TestFixture]
    public class RenderingComparerTest
    {
        private RenderingComparer _comparer;
        
        [SetUp]
        public void Setup()
        {
            _comparer = new RenderingComparer();    
        }
        
        [Test]
        public void TestComparerReturnsDefaultWhenEqual()
        {
            var value = Create(5, 5);
            Assert.AreEqual(_comparer.Compare(value, value), 0);
        }
        
        [Test]
        public void TestComparerReturnsNearest()
        {
            var f0 = Create(20, 20);
            var f1 = Create(-5, -5);
            Assert.AreEqual(_comparer.Compare(f0, f1), 1);
            Assert.AreEqual(_comparer.Compare(f1, f0), -1);
        }

        private static KeyValuePair<Vector2, ChunkRenderCommand> Create(float X, float Y)
        {
            return new KeyValuePair<Vector2, ChunkRenderCommand>(
                new Vector2(X, Y),
                new ChunkRenderCommand
                {
                    DrawCount = 0,
                    Entries = new MemoryEntry[0],
                    VertexCount = 0
                }
            );
        }
    }
}