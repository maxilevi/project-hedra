using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.UnitTesting.GenerationTests
{
    public class WorldTester : BaseTest
    {
        [TestMethod]
        public void TestIsChunkOffset()
        {
            var realChunkOffset = new Vector2(Chunk.ChunkWidth * 560, Chunk.ChunkWidth * 421);
            this.AssertTrue(World.IsChunkOffset(realChunkOffset));

            var fakeChunkOffset = new Vector2(Chunk.ChunkWidth * 560 + 32, Chunk.ChunkWidth * 421 + 13);
            this.AssertFalse(World.IsChunkOffset(fakeChunkOffset));
        }

        [TestMethod]
        public void TestBlockSpace()
        {

            var originalVector = new Vector3(Chunk.ChunkWidth * 560 + Chunk.ChunkWidth / 2, 0, Chunk.ChunkWidth * 421 + Chunk.ChunkWidth / 2);
            this.AssertEqual(
                World.ToBlockSpace(originalVector),
                new Vector3(Chunk.ChunkWidth / 2 / Chunk.BlockSize, 0, Chunk.ChunkWidth / 2 / Chunk.BlockSize)
                );
        }

        [TestMethod]
        public void TestChunkSpace()
        {
            var originalVector = new Vector3(Chunk.ChunkWidth * 560 + Chunk.ChunkWidth / 2, 0, Chunk.ChunkWidth * 421 + Chunk.ChunkWidth / 2);
            this.AssertEqual(
                World.ToChunkSpace(originalVector),
                new Vector2(Chunk.ChunkWidth * 560, Chunk.ChunkWidth * 421)
            );
        }
    }
}
