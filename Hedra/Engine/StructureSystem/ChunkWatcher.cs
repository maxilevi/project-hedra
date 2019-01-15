using System;
using System.Collections.Generic;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public abstract class ChunkWatcher<T1> : IDisposable where T1 : ISearchable
    {
        private readonly object _lock = new object();
        private readonly Dictionary<T1, Chunk> _added;
        
        protected ChunkWatcher()
        {
            _added = new Dictionary<T1, Chunk>();
            World.OnChunkReady += OnChunkReady;
            World.OnChunkDisposed += OnChunkDisposed;
        }

        protected abstract void Add(Chunk Object, T1 Value);

        protected abstract void Delete(Chunk Object, T1 Value);

        protected abstract T1[] Get();

        private void AddIfNecessary(T1 Value, Chunk Object)
        {
            lock (_lock)
            {
                if (!Object.Disposed && Object.Initialized && (!_added.ContainsKey(Value) || _added[Value] != Object))
                {
                    Add(Object, Value);
                    if (_added.ContainsKey(Value))
                        _added[Value] = Object;
                    else
                        _added.Add(Value, Object);
                }
            }
        }

        public void OnChunkReady(Chunk Object)
        {
            bool IsInRange(Vector3 Position)
            {
                var chunkSpace = World.ToChunkSpace(Position);
                return Object.OffsetX == (int) chunkSpace.X && Object.OffsetZ == (int) chunkSpace.Y;
            }
            var objects = Get();
            for (var i = 0; i < objects.Length; i++)
            {
                if(IsInRange(objects[i].Position))
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
                    if(pair.Value != null)
                        Delete(pair.Value, pair.Key);
                }
                _added.Clear();
            }
            World.OnChunkReady -= this.OnChunkReady;
            World.OnChunkDisposed -= this.OnChunkDisposed;
        }
    }
}