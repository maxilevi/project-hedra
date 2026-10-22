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
        private readonly Action _completed;

        public Worker(Action Completed = null)
        {
            _completed = Completed;
            _resetEvent = new AutoResetEvent(false);
            _workerThread = new Thread(Update, MB * 4)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();
        }

        public bool IsWorking => Volatile.Read(ref _action) != null;
        public object Owner { get; private set; }

        public void Update()
        {
            while (!GameSettings.TestingMode && (Engine.Program.GameWindow?.Exists ?? false))
            {
                _resetEvent.WaitOne();
                try
                {
                    Volatile.Read(ref _action)?.Invoke();
                }
                catch (Exception e)
                {
                    Log.WriteLine($"Worker failed with status -1: {Environment.NewLine}'{e}' ");
                }
                finally
                {
                    Owner = null;
                    Volatile.Write(ref _action, null);
                    _completed?.Invoke();
                }
            }
        }

        public void Do(Action Job, object Owner)
        {
            this.Owner = Owner;
            Volatile.Write(ref _action, Job);
            _resetEvent.Set();
        }
    }
}
