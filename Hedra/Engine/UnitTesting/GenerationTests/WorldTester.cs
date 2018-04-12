using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.UnitTesting.GenerationTests
{
    public class WorldTester : BaseTest
    {
        [TestMethod]
        public void TestIsChunkOffset()
        {
            var realChunkOffset = new Vector2(Chunk.Width * 560, Chunk.Width * 421);
            this.AssertTrue(World.IsChunkOffset(realChunkOffset));

            var fakeChunkOffset = new Vector2(Chunk.Width * 560 + 32, Chunk.Width * 421 + 13);
            this.AssertFalse(World.IsChunkOffset(fakeChunkOffset));
        }

        [TestMethod]
        public void TestBlockSpace()
        {

            var originalVector = new Vector3(Chunk.Width * 560 + Chunk.Width / 2, 0, Chunk.Width * 421 + Chunk.Width / 2);
            this.AssertEqual(
                World.ToBlockSpace(originalVector),
                new Vector3(Chunk.Width / 2 / Chunk.BlockSize, 0, Chunk.Width / 2 / Chunk.BlockSize)
                );
        }

        [TestMethod]
        public void TestChunkSpace()
        {
            var originalVector = new Vector3(Chunk.Width * 560 + Chunk.Width / 2, 0, Chunk.Width * 421 + Chunk.Width / 2);
            this.AssertEqual(
                World.ToChunkSpace(originalVector),
                new Vector2(Chunk.Width * 560, Chunk.Width * 421)
            );
        }
    }
}
