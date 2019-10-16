using System.Collections.Generic;
using Hedra.Engine;
using Hedra.Engine.Core;
using Hedra.Engine.Pathfinding;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class TraverseStorage : Singleton<TraverseStorage>
    {
        private readonly Dictionary<IEntity, GridStorage> _storage = new Dictionary<IEntity, GridStorage>();
        private bool _init;

        private void Init()
        {
            _init = true;
        }
        
        public void RebuildIfNecessary(IEntity Parent, bool NewTarget = false)
        {
            _storage[Parent].RebuildIfNecessary(Parent, NewTarget);
        }

        public void Update(IEntity Parent)
        {
            if (!_init) Init();
            _storage[Parent].Update();
        }

        public void ResetTime(IEntity Parent)
        {
            _storage[Parent].ResetTime();
        }
        
        public void CreateIfNecessary(IEntity Parent, OnGridUpdated GridUpdated)
        {
            if (!_storage.ContainsKey(Parent))
                _storage.Add(Parent, new GridStorage(Create(Parent)));
            else
                _storage[Parent].ReferenceCounter++;
            _storage[Parent].GridUpdated += GridUpdated;
        }

        public void RemoveIfNecessary(IEntity Parent, OnGridUpdated GridUpdated)
        {
            _storage[Parent].GridUpdated -= GridUpdated;
            if (_storage[Parent].ReferenceCounter == 1)
                _storage.Remove(Parent);
            else
                _storage[Parent].ReferenceCounter--;
        }

        public void ResizeGrid(IEntity Parent, Vector2 Size)
        {
            _storage[Parent].Storage = new Grid((int)Size.X, (int)Size.Y);
        }
        
        private static Grid Create(IEntity Parent)
        {
            var size = 16 + (int)(Parent.Size.LengthFast() / 4);
            return new Grid(size, size);
        }
        
        public Grid this[IEntity Parent] => _storage[Parent].Storage;
    }
}