using System;
using System.Collections.Generic;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem.Builders;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class SharedWorkerPool : WorkerPool
    {
        private readonly Dictionary<QueueType, int> _maxWorkers;

        public SharedWorkerPool(int WorkerCount = MaxWorkers) : base(WorkerCount)
        {
            _maxWorkers = new Dictionary<QueueType, int>();
        }

        public void SetMaxWorkers(QueueType Type, int Max)
        {
            lock (PoolLock) _maxWorkers[Type] = Max;
        }

        public int GetMaxWorkers(QueueType Type)
        {
            lock (PoolLock) return _maxWorkers.TryGetValue(Type, out var max) ? max : Workers.Count;
        }

        private int GetCurrentWorkers(ICountable User)
        {
            var count = 0;
            for (var i = 0; i < Workers.Count; i++)
                if (Workers[i].Owner == User)
                    count++;
            return count;
        }

        public virtual bool Work(ICountable User, QueueType Type, Action Do)
        {
            lock (PoolLock)
            {
                var maxWorkers = GetMaxWorkers(Type);
                if (GetCurrentWorkers(User) >= maxWorkers) return false;
                return base.Work(Do, User);
            }
        }
    }
}
