using Hedra;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Generation
{
    [TestFixture]
    public class WorldTest
    {
        [SetUp]
        public void Setup()
        {
            World.Provider = new WorldProvider();
        }
        
        [Test]
        public void TestIsChunkOffset()
        {
            var realChunkOffset = new Vector2(Chunk.Width * 560, Chunk.Width * 421);
            Assert.True(World.IsChunkOffset(realChunkOffset));

            var fakeChunkOffset = new Vector2(Chunk.Width * 560 + 32, Chunk.Width * 421 + 13);
            Assert.False(World.IsChunkOffset(fakeChunkOffset));
        }

        [Test]
        public void TestBlockSpace()
        {
            var originalVector = new Vector3(Chunk.Width * 560 + Chunk.Width / 2, 0, Chunk.Width * 421 + Chunk.Width / 2);
            Assert.AreEqual(
                World.ToBlockSpace(originalVector),
                new Vector3(Chunk.Width / 2.0f / Chunk.BlockSize, 0, Chunk.Width / 2.0f / Chunk.BlockSize)
            );
        }

        [Test]
        public void TestChunkSpace()
        {
            var originalVector = new Vector3(Chunk.Width * 560 + Chunk.Width / 2, 0, Chunk.Width * 421 + Chunk.Width / 2);
            Assert.AreEqual(
                World.ToChunkSpace(originalVector),
                new Vector2(Chunk.Width * 560, Chunk.Width * 421)
            );
        }
    }
}