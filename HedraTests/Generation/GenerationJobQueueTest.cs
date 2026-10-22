using System;
using System.Collections.Generic;
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;
using NUnit.Framework;

namespace HedraTests.Generation
{
    public class GenerationJobQueueTest
    {
        private sealed class Pool : SharedWorkerPool
        {
            public readonly Queue<Action> Started = new();
            public bool Accept = true;
            public Pool() : base(0) { }
            public override bool Work(ICountable User, QueueType Type, Action Do)
            {
                Assert.AreEqual(QueueType.Structures, Type);
                if (!Accept) return false;
                Started.Enqueue(Do);
                return true;
            }
        }

        [Test]
        public void ReadinessIncludesActiveJobsAfterQueueBecomesEmpty()
        {
            var pool = new Pool();
            var queue = new GenerationJobQueue(pool);
            var built = false;
            queue.Enqueue(() => built = true);
            Assert.True(queue.TryDispatch());
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(1, queue.PendingCount);
            Assert.False(built);
            pool.Started.Dequeue()();
            Assert.True(built);
            Assert.AreEqual(0, queue.PendingCount);
        }

        [Test]
        public void RejectedDispatchKeepsJobQueued()
        {
            var pool = new Pool { Accept = false };
            var queue = new GenerationJobQueue(pool);
            queue.Enqueue(() => { });
            Assert.False(queue.TryDispatch());
            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual(1, queue.PendingCount);
            pool.Accept = true;
            Assert.True(queue.TryDispatch());
            pool.Started.Dequeue()();
            Assert.AreEqual(0, queue.PendingCount);
        }

        [Test]
        public void DiscardRemovesQueuedJobsAndWaitsForActiveJob()
        {
            var pool = new Pool();
            var queue = new GenerationJobQueue(pool);
            queue.Enqueue(() => { });
            queue.Enqueue(() => Assert.Fail("Discarded job ran"));
            queue.TryDispatch();
            queue.Discard();
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(1, queue.PendingCount);
            pool.Started.Dequeue()();
            Assert.AreEqual(0, queue.PendingCount);
        }

        [Test]
        public void FailedJobReleasesItsPendingCount()
        {
            var pool = new Pool();
            var queue = new GenerationJobQueue(pool);
            queue.Enqueue(() => throw new InvalidOperationException());
            queue.TryDispatch();
            Assert.Throws<InvalidOperationException>(() => pool.Started.Dequeue()());
            Assert.AreEqual(0, queue.PendingCount);
        }
    }
}
