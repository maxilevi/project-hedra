using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.Management
{
    public class TickSystem
    {
        public int UpdatesPerSecond { get; set; } = 5;
        private readonly List<ITickable> _tickables;
        private float _counter;

        public TickSystem()
        {
            _tickables = new List<ITickable>();
        }

        public void Add(ITickable Tickable)
        {
            if(!(Tickable is IUpdatable)) throw new ArgumentException("ITickable also needs to be IUpdatable");

            lock (_tickables)
                _tickables.Add(Tickable);
        }

        public void Remove(ITickable Tickable)
        {
            if (!(Tickable is IUpdatable)) throw new ArgumentException("ITickable also needs to be IUpdatable");

            lock (_tickables)
                _tickables.Remove(Tickable);
        }

        public void Tick()
        {
            if (_counter > 1.0 / UpdatesPerSecond)
            {
                lock (_tickables)
                {
                    for (int i = _tickables.Count - 1; i >= 0; i--)
                    {
                        (_tickables[i] as IUpdatable)?.Update();
                    }
                }
                _counter = 0;
            }
            _counter += Time.IndependentDeltaTime;
        }
    }
}
