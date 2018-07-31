using System.Collections.Generic;
using System;

namespace Hedra.Engine.Management
{
    public class DistributedExecuter
    {
        public const int ExecutionsPerFrame = 1;
        private static readonly Queue<Action> _jobs;
        private static readonly object _lock;

        static DistributedExecuter()
        {
            _jobs = new Queue<Action>();
            _lock = new object();
        }

        public static void Update()
        {
            lock (_lock)
            {
                for (var i = 0; i < ExecutionsPerFrame; i++)
                {
                    if (_jobs.Count == 0) return;
                    _jobs.Dequeue().Invoke();
                }
            }
        }

        public static void Execute(Action Job)
        {
            lock (_lock)
            {
                _jobs.Enqueue(Job);
            }
        }
    }
}
