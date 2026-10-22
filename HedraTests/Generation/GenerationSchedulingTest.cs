using System.Numerics;
using System.Runtime.Serialization;
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;
using Hedra.Game;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [TestFixture, NonParallelizable]
    public class GenerationSchedulingTest
    {
        private sealed class QueueOwner : ICountable { public int Count => 1; }
        private bool _wasTesting;

        [SetUp]
        public void Setup()
        {
            _wasTesting = GameSettings.TestingMode;
            GameSettings.TestingMode = true;
        }

        [TearDown]
        public void Teardown() => GameSettings.TestingMode = _wasTesting;

        [Test]
        public void BlockJobsCannotConsumeReservedMeshWorker()
        {
            var pool = new SharedWorkerPool(2);
            var blocks = new QueueOwner();
            var meshes = new QueueOwner();
            pool.SetMaxWorkers(QueueType.Blocks, 1);
            pool.SetMaxWorkers(QueueType.Meshing, 1);
            Assert.True(pool.Work(blocks, QueueType.Blocks, () => { }));
            Assert.False(pool.Work(blocks, QueueType.Blocks, () => { }));
            Assert.True(pool.Work(meshes, QueueType.Meshing, () => { }));
        }

        [Test]
        public void DisabledQueueDoesNotDispatchAndCanBeReenabled()
        {
            var pool = new SharedWorkerPool(1);
            var owner = new QueueOwner();
            pool.SetMaxWorkers(QueueType.Blocks, 0);
            Assert.False(pool.Work(owner, QueueType.Blocks, () => { }));
            pool.SetMaxWorkers(QueueType.Blocks, 1);
            Assert.True(pool.Work(owner, QueueType.Blocks, () => { }));
        }

        // Lookup tests only need chunk identity/lifetime, not terrain or a GL context.
        private static Chunk EmptyChunk() => (Chunk)FormatterServices.GetUninitializedObject(typeof(Chunk));

        [Test]
        public void ChunkLookupMissDoesNotHideNewNeighbour()
        {
            var provider = new WorldProvider();
            Assert.Null(provider.GetChunkByOffset(128, 256));
            var chunk = EmptyChunk();
            provider.SearcheableChunks.Add(new Vector2(128, 256), chunk);
            Assert.AreSame(chunk, provider.GetChunkByOffset(128, 256));
        }

        [Test]
        public void ChunkLookupCacheDoesNotCrossWorldProviders()
        {
            var first = new WorldProvider();
            var second = new WorldProvider();
            var chunk = EmptyChunk();
            first.SearcheableChunks.Add(Vector2.Zero, chunk);
            Assert.AreSame(chunk, first.GetChunkByOffset(0, 0));
            Assert.Null(second.GetChunkByOffset(0, 0));
        }

        [Test]
        public void DisposedChunkIsNotReturnedFromCacheAfterRemoval()
        {
            var provider = new WorldProvider();
            var chunk = EmptyChunk();
            provider.SearcheableChunks.Add(Vector2.Zero, chunk);
            Assert.AreSame(chunk, provider.GetChunkByOffset(0, 0));
            chunk.Dispose();
            provider.SearcheableChunks.Remove(Vector2.Zero);
            Assert.Null(provider.GetChunkByOffset(0, 0));
        }
    }
}
