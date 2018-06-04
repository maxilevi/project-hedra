using System.Collections.Generic;
using System.Threading;

namespace Hedra.Engine.PhysicsSystem
{
    public class SchedulerWorker
    {
        private readonly Dictionary<uint, PhysicsListener> _listeners;
        private readonly Thread _processingThread;
        private bool _sleep;

        public SchedulerWorker(Dictionary<uint, PhysicsListener> Listeners)
        {
            _processingThread = new Thread(this.Work);
            _listeners = Listeners;
            _processingThread.Start();
        }

        private void Work()
        {
            while (Program.GameWindow.Exists)
            {
                if (_sleep)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var collided = false;
                KeyValuePair<uint, PhysicsListener>[] listenerCopy;
                lock (_listeners)
                {
                    listenerCopy = _listeners.ToArray();
                }
                foreach (var pair in listenerCopy)
                {
                    var listener = pair.Value;
                    var shapes0 = listener.Shapes0();
                    var shapes1 = listener.Shapes1();
                    for (var i = 0; i < shapes0.Length && !collided; i++)
                    {
                        for (var j = 0; j < shapes1.Length && !collided; j++)
                        {
                            if (!Physics.Collides(shapes0[i], shapes1[j])) continue;
                            lock (_listeners)
                            {
                                _listeners.Remove(pair.Key);
                            }
                            listener.Callback();
                            collided = true;
                        }
                    }
                }
                _sleep = true;
            }
        }

        public void Wakeup()
        {
            _sleep = false;
        }
    }
}
