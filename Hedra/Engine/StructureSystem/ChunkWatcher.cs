using System;
using System.Collections.Generic;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem
{
    public abstract class ChunkWatcher<T1> : IDisposable where T1 : ISearchable
    {
        private readonly object _lock = new object();
        private readonly Dictionary<T1, Chunk> _added;
        private bool _addedAfterCreation;
        
        protected ChunkWatcher()
        {
            _added = new Dictionary<T1, Chunk>();
            World.OnChunkReady += OnChunkReady;
            World.OnChunkDisposed += OnChunkDisposed;
        }

        protected abstract void Add(Chunk Object, T1 Value);

        protected abstract void Delete(Chunk Object, T1 Value);

        protected abstract T1[] Get();

        private void AddAfterCreationIfNecessary(T1[] Values)
        {
            if(_addedAfterCreation) return;
            _addedAfterCreation = true;
            for (var i = 0; i < Values.Length; ++i)
            {
                var chunk = World.GetChunkAt(Values[i].Position);
                if(chunk != null) AddIfNecessary(Values[i], chunk);
            }
        }

        private void AddIfNecessary(T1 Value, Chunk Object)
        {
            lock (_lock)
            {
                if (!Object.Disposed && (!_added.ContainsKey(Value) || _added[Value] != Object))
                {
                    Add(Object, Value);
                    if (_added.ContainsKey(Value))
                        _added[Value] = Object;
                    else
                        _added.Add(Value, Object);
                }
            }
        }
        
        private static bool IsInRange(Chunk Object, Vector3 Position)
        {
            var chunkSpace = World.ToChunkSpace(Position);
            return Object.OffsetX == (int) chunkSpace.X && Object.OffsetZ == (int) chunkSpace.Y;
        }

        public void OnChunkReady(Chunk Object)
        {
            var objects = Get();
            if(!_addedAfterCreation) AddAfterCreationIfNecessary(objects);
            
            for (var i = 0; i < objects.Length; i++)
            {
                if(IsInRange(Object, objects[i].Position))
                    AddIfNecessary(objects[i], Object);          
            }
        }
      
        private void OnChunkDisposed(Chunk Object)
        {
            KeyValuePair<T1, Chunk>[] dict;
            lock (_lock)
            {
                dict = _added.ToArray();
            }
            foreach (var pair in dict)
            {
                if (pair.Value == Object)
                    _added[pair.Key] = null;
            }
        }
        
        public void Dispose()
        {
            lock (_lock)
            {
                foreach(var pair in _added)
                {
                    if(pair.Value != null && !pair.Value.Disposed)
                        Delete(pair.Value, pair.Key);
                }
                _added.Clear();
            }
            World.OnChunkReady -= this.OnChunkReady;
            World.OnChunkDisposed -= this.OnChunkDisposed;
        }
    }
}