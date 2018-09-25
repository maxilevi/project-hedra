using System;
using System.Collections.Generic;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class WorkerPool
    {
        public const int MaxWorkers = 2;
        protected readonly List<Worker> Workers;

        public WorkerPool(int WorkerCount)
        {
            Workers = new List<Worker>();
            for(var i = 0; i < WorkerCount; i++)
            {
                Workers.Add(new Worker());
            }
        }

        public virtual bool Work(Action Job, object Owner, int SleepTime)
        {
            var worker = this.GetAvailableWorker();
            if (worker != null)
            {
                worker.SleepTime = SleepTime;
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
