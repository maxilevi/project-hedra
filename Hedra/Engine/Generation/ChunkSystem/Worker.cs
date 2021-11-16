using System;
using System.Threading;
using Hedra.Engine.IO;
using Hedra.Game;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class Worker
    {
        private const int MB = 1024 * 1024;
        private readonly AutoResetEvent _resetEvent;
        private Action _action;
        private readonly Thread _workerThread;

        public Worker()
        {
            _resetEvent = new AutoResetEvent(false);
            _workerThread = new Thread(Update, MB * 4)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            _workerThread.Start();
        }

        public bool IsWorking => _action != null;
        public object Owner { get; private set; }

        public void Update()
        {
            while (GameManager.Exists && !GameSettings.TestingMode)
            {
                _resetEvent.WaitOne();
                try
                {
                    _action.Invoke();
                    Owner = null;
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
            this.Owner = Owner;
            _resetEvent.Set();
        }
    }
}