using System;
using System.Collections.Generic;

namespace Hedra.Engine.Generation.ChunkSystem
{
    internal class WorkerPool
    {
        public const int MaxWorkers = 2;
        protected readonly List<Worker> Workers;

        public WorkerPool()
        {
            Workers = new List<Worker>();
            for(var i = 0; i < MaxWorkers; i++)
            {
                Workers.Add(new Worker());
            }
        }

        public virtual bool Work(Action Job, object Owner)
        {
            var worker = this.GetAvailableWorker();
            if (worker != null)
            {
                worker.Do(Job, Owner);
                return true;
            }
            return false;
        }

        protected Worker GetAvailableWorker()
        {
            for (var i = 0; i < Workers.Count; i++)
            {
                if (!Workers[i].IsWorking) return Workers[i];
            }
            return null;
        }
    }
}
