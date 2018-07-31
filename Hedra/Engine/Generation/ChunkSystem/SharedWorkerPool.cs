using System;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class SharedWorkerPool : WorkerPool
    {
        private readonly LoadBalancer _balancer;

        public SharedWorkerPool()
        {
            _balancer = new LoadBalancer(WorkerPool.MaxWorkers);
        }

        public void Register(ICountable User)
        {
            _balancer.Register(User);
        }

        public void Unregister(ICountable User)
        {
            _balancer.Unregister(User);
        }

        private int GetCurrentWorkers(ICountable User)
        {
            var count = 0;
            for (var i = 0; i < Workers.Count; i++)
            {
                if(Workers[i].Owner == User) count++;
            }
            return count;
        }

        public bool Work(ICountable User, Action Do, int SleepTime)
        {
            if (!_balancer.Contains(User))
                throw new ArgumentOutOfRangeException($"User hasn't been registed in the shared pool.");
            var maxWorkers = _balancer[User];
            var currentWorkers = this.GetCurrentWorkers(User);
            if (currentWorkers >= maxWorkers) return false;
            return base.Work(Do, User, SleepTime);       
        }
    }
}
