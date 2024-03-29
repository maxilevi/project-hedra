using System;
using System.Collections.Generic;

namespace Hedra.Engine.Management
{
    public abstract class StateManager
    {
        protected readonly Dictionary<TrackItem, object> _cache;
        protected readonly HashSet<TrackItem> _trackItems;
        protected bool _state;

        protected StateManager()
        {
            _trackItems = new HashSet<TrackItem>();
            _cache = new Dictionary<TrackItem, object>();
        }

        protected void RegisterStateItem(Func<object> Getter, Action<object> Setter, bool ReleaseFirst = false)
        {
            if (_state) throw new ArgumentException("A state cannot be registeres while the manager is active.");
            _trackItems.Add(new TrackItem(Getter, Setter, ReleaseFirst));
        }

        public virtual void CaptureState()
        {
            if (_state) throw new StackOverflowException("Cannot capture a state while there already is one in memory");
            _state = true;

            foreach (var item in _trackItems) _cache.Add(item, item.Getter.Invoke());
        }

        public virtual void ReleaseState()
        {
            if (!_state) throw new InvalidOperationException("Cannot release an empty state.");
            _state = false;

            foreach (var cacheItem in _cache) cacheItem.Key.Setter.Invoke(cacheItem.Value);
            _cache.Clear();
        }

        public bool GetState()
        {
            return _state;
        }
    }
}