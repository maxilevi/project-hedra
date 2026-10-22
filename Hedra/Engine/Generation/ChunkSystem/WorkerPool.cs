using System;
using System.Collections.Generic;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class WorkerPool
    {
        public const int MaxWorkers = 2;
        protected readonly List<Worker> Workers;
        protected readonly object PoolLock = new();
        public event Action WorkerAvailable;

        public WorkerPool(int WorkerCount)
        {
            Workers = new List<Worker>();
            for (var i = 0; i < WorkerCount; i++) Workers.Add(new Worker(() => WorkerAvailable?.Invoke()));
        }

        public virtual bool Work(Action Job, object Owner)
        {
            lock (PoolLock)
            {
                var worker = GetAvailableWorker();
                if (worker == null) return false;
                worker.Do(Job, Owner);
                return true;
            }
        }

        protected Worker GetAvailableWorker()
        {
            for (var i = 0; i < Workers.Count; i++)
                if (!Workers[i].IsWorking)
                    return Workers[i];
            return null;
        }
    }
}
