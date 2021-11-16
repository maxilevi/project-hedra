using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Scenes;
using Hedra.EntitySystem;
using Hedra.Framework;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class TraverseStorage : Singleton<TraverseStorage>
    {
        private readonly object _lock = new object();
        private readonly Dictionary<IEntity, GridStorage> _storage = new Dictionary<IEntity, GridStorage>();
        private bool _init;

        public WaypointGrid this[IEntity Parent]
        {
            get
            {
                lock (_lock)
                {
                    return _storage[Parent].Storage;
                }
            }
        }

        public int StorageCount
        {
            get
            {
                lock (_lock)
                {
                    return _storage.Count;
                }
            }
        }

        private void Init()
        {
            _init = true;
        }

        public bool RebuildIfNecessary(IEntity Parent, bool NewTarget = false)
        {
            lock (_lock)
            {
                return _storage[Parent].RebuildIfNecessary(Parent, NewTarget);
            }
        }

        public void Update(IEntity Parent)
        {
            if (!_init) Init();
            lock (_lock)
            {
                if (!_storage.ContainsKey(Parent)) return;
                _storage[Parent].Update();
            }
        }

        public void ResetTime(IEntity Parent)
        {
            lock (_lock)
            {
                _storage[Parent].ResetTime();
            }
        }

        public void CreateIfNecessary(IEntity Parent, OnGridUpdated GridUpdated)
        {
            lock (_lock)
            {
                if (!_storage.ContainsKey(Parent))
                    _storage.Add(Parent, new GridStorage(Create(Parent)));
                else
                    _storage[Parent].ReferenceCounter++;
                _storage[Parent].GridUpdated += GridUpdated;
            }
        }

        public void RemoveIfNecessary(IEntity Parent, OnGridUpdated GridUpdated)
        {
            lock (_lock)
            {
                if (!_storage.ContainsKey(Parent)) return;
                _storage[Parent].GridUpdated -= GridUpdated;
                if (_storage[Parent].ReferenceCounter == 1)
                    _storage.Remove(Parent);
                else
                    _storage[Parent].ReferenceCounter--;
            }
        }

        public void ResizeGrid(IEntity Parent, Vector2 Size)
        {
            lock (_lock)
            {
                _storage[Parent].Storage = new WaypointGrid((int)Size.X, (int)Size.Y);
            }
        }

        private static WaypointGrid Create(IEntity Parent)
        {
            var size = 16 + (int)(Parent.Size.LengthFast() / 4);
            return new WaypointGrid(size, size);
        }
    }
}