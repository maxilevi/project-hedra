using System;
using System.Threading;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Game;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class Worker
    {
        private Thread _workerThread;
        private object _owner;
        private Action _action;

        public Worker()
        {
            _workerThread = new Thread(this.Update)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            _workerThread.Start();
        }

        public void Update()
        {
            while (GameManager.Exists && !GameSettings.TestingMode)
            {
                Thread.Sleep(SleepTime);
                if (!IsWorking) continue;
                try
                {
                    _action.Invoke();
                    _owner = null;
                    _action = null;
                }
                catch (Exception e)
                {
                    Log.WriteLine($"Worker failed with status -1: {Environment.NewLine}'{e}' ");
                }
            }
        }

        public void Do(Action Job, object Owner)
        {
            _action = Job;
            _owner = Owner;
        }

        public int SleepTime { get; set; } = 5;
        public bool IsWorking => _action != null;
        public object Owner => _owner;
    }
}