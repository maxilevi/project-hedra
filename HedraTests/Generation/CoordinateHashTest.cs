using System;
using Hedra.Engine.Generation.ChunkSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Generation
{
    [TestFixture]
    public class CoordinateHashTest
    {
        private readonly Random _rng;

        public CoordinateHashTest()
        {
            _rng = new Random();
        }
        
        [Test]
        public void TestEqual()
        {
            var value = RandomVector;
            var hash1 = new CoordinateHash3D(value);
            var hash2 = new CoordinateHash3D(value);
            Assert.AreEqual(hash1, hash2);
            Assert.True(hash1.Equals(hash2));
        }

        [Test]
        public void TestNotEqual()
        {
            var value = RandomVector;
            var hash1 = new CoordinateHash3D(Vector3.One + value);
            var hash2 = new CoordinateHash3D(value);
            Assert.AreNotEqual(hash1, hash2);
            Assert.False(hash1.Equals(hash2));
        }

        [Test]
        public void TestToVector3()
        {
            var value = RandomVector;
            Assert.AreEqual(value, new CoordinateHash3D(value).ToVector3());
        }

        [Test]
        public void TestRange()
        {
            var value = new Vector3(Chunk.Width / Chunk.BlockSize - 1, Chunk.Height / Chunk.BlockSize - 1, Chunk.Width / Chunk.BlockSize - 1);
            Assert.AreEqual(value, new CoordinateHash3D(value).ToVector3());
        }

        private Vector3 RandomVector => 
            new Vector3(
                _rng.Next(0, CoordinateHash3D.MaxCoordinateSizeXZ),
                _rng.Next(0, CoordinateHash3D.MaxCoordinateSizeY),
                _rng.Next(0, CoordinateHash3D.MaxCoordinateSizeXZ)
            );
    }
}