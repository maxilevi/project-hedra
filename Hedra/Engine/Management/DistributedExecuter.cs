using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace Hedra.Engine.Management
{
    public class DistributedExecuter
    {
        public const int ExecutionsPerFrame = 8;
        private static readonly ConcurrentQueue<Action> _jobs;

        static DistributedExecuter()
        {
            _jobs = new ConcurrentQueue<Action>();
        }

        public static void Update()
        {
            for (var i = 0; i < ExecutionsPerFrame; i++)
            {
                if (_jobs.Count == 0) return;
                var result = _jobs.TryDequeue(out var job);
                if(result) 
                    job.Invoke();
            }
        }

        public static void Execute(Action Job)
        {
            _jobs.Enqueue(Job);
        }
    }
}
