using System;
using System.Collections.Generic;
using System.Threading;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;

namespace Hedra.Engine.Generation
{
    public sealed class GenerationJobQueue : ICountable
    {
        private readonly SharedWorkerPool _pool;
        private readonly Queue<Action> _jobs = new();
        private readonly object _lock = new();
        private int _pending;
        public GenerationJobQueue(SharedWorkerPool Pool) => _pool = Pool;
        public int Count { get { lock (_lock) return _jobs.Count; } }
        public int PendingCount => Volatile.Read(ref _pending);

        public void Enqueue(Action Job)
        {
            lock (_lock)
            {
                Interlocked.Increment(ref _pending);
                _jobs.Enqueue(Job);
            }
        }

        public bool TryDispatch()
        {
            lock (_lock)
            {
                if (_jobs.Count == 0) return false;
                var job = _jobs.Peek();
                if (!_pool.Work(this, QueueType.Structures, () =>
                    {
                        try { job(); }
                        finally { Interlocked.Decrement(ref _pending); }
                    })) return false;
                _jobs.Dequeue();
                return true;
            }
        }

        public void Discard()
        {
            lock (_lock)
            {
                Interlocked.Add(ref _pending, -_jobs.Count);
                _jobs.Clear();
            }
        }
    }
}
