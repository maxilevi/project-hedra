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
        private readonly AutoResetEvent _resetEvent;
        private object _owner;
        private Action _action;
        private const int MB = 1024 * 1024;

        public Worker()
        {
            _resetEvent = new AutoResetEvent(false);
            _workerThread = new Thread(this.Update, MB * 4)
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
                _resetEvent.WaitOne();
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
            _resetEvent.Set();
        }

        public bool IsWorking => _action != null;
        public object Owner => _owner;
    }
}