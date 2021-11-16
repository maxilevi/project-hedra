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
            _maxWorkers[Type] = Max;
        }

        public int GetMaxWorkers(QueueType Type)
        {
            return _maxWorkers[Type];
        }

        private int GetCurrentWorkers(ICountable User)
        {
            var count = 0;
            for (var i = 0; i < Workers.Count; i++)
                if (Workers[i].Owner == User)
                    count++;
            return count;
        }

        public bool Work(ICountable User, QueueType Type, Action Do)
        {
            var maxWorkers = _maxWorkers[Type];
            var currentWorkers = GetCurrentWorkers(User);
            if (currentWorkers >= maxWorkers) return false;
            return base.Work(Do, User);
        }
    }
}