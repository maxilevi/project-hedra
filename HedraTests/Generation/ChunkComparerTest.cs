using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Generation
{
    [TestFixture]
    public class ChunkComparerTest
    {
        private ChunkComparer _comparer;
        
        [SetUp]
        public void Setup()
        {
            _comparer = new ChunkComparer();    
        }
        
        [Test]
        public void TestComparerReturnsDefaultWhenEqual()
        {
            Assert.AreEqual(_comparer.Compare(new PositionableMock(128, 128), new PositionableMock(128, 128)), 0);
        }
        
        [Test]
        public void TestComparerReturnsNearest()
        {
            var f0 = new PositionableMock(256, 256);
            var f1 = new PositionableMock(-128, -128);
            Assert.AreEqual(_comparer.Compare(f0, f1), 1);
            Assert.AreEqual(_comparer.Compare(f1, f0), -1);
        }
        
        class PositionableMock : IPositionable
        {
            public Vector3 Position { get; }
        
            public PositionableMock(int X, int Z)
            {
                this.Position = new Vector3(X, 0, Z);
            }
        }
    }
}