using System.Collections.Generic;
using Hedra.Core;

namespace Hedra.Engine.Management
{
    public class TickSystem
    {
        private readonly object _lock;
        private readonly Dictionary<ITickable, TickInformation> _tickables;
        private readonly List<ITickable> _toRemove;

        public TickSystem()
        {
            _lock = new object();
            _tickables = new Dictionary<ITickable, TickInformation>();
            _toRemove = new List<ITickable>();
        }

        public void Add(ITickable Tickable)
        {
            lock (_lock)
            {
                _tickables.Add(Tickable, new TickInformation
                {
                    AlertTime = Tickable.UpdatesPerSecond
                });
            }
        }

        public void Remove(ITickable Tickable)
        {
            lock (_lock)
            {
                _toRemove.Add(Tickable);
            }
        }

        public void Tick()
        {
            lock (_lock)
            {
                RemovePending();
                foreach (var pair in _tickables)
                {
                    var tickable = pair.Key;
                    var information = pair.Value;
                    if(information.Tick())
                        tickable.Update(information.AlertTime);
                }
            }
        }

        private void RemovePending()
        {
            for (var i = 0; i < _toRemove.Count; ++i)
            {
                _tickables.Remove(_toRemove[i]);
            }
            _toRemove.Clear();
        }
        
        private class TickInformation
        {
            private float _alertTime;
            public float AlertTime
            {
                get => _alertTime;
                set => _alertTime = 1f / value;
            }
            private float _counter;

            public bool Tick()
            {
                _counter += Time.IndependentDeltaTime;
                if (_counter >= _alertTime)
                {
                    _counter = 0;
                    return true;
                }

                return false;
            }
        }
    }
}
