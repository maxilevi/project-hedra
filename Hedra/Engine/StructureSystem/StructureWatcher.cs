using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class StructureWatcher : IDisposable
    {
        public CollidableStructure Structure { get; }
        private readonly object _lock = new object();
        private readonly Dictionary<CachedVertexData, Chunk> _chunksAdded;

        public StructureWatcher(CollidableStructure Structure)
        {
            this.Structure = Structure;
            _chunksAdded = new Dictionary<CachedVertexData, Chunk>();
            Structure.ModelAdded += this.ModelAdded;
            World.OnChunkReady += this.OnChunkReady;
            World.OnChunkDisposed += this.OnChunkDisposed;
        }

        private void OnChunkReady(Chunk Object)
        {
            var models = Structure.Models;
            for (var i = 0; i < models.Length; i++)
            {
                var chunkSpace = World.ToChunkSpace(models[i].Position);
                if (Object.OffsetX == (int) chunkSpace.X && Object.OffsetZ == (int) chunkSpace.Y)
                {
                    AddIfNecessary(models[i], Object);
                }
            }
        }

        private void ModelAdded(CachedVertexData Model)
        {
            var chunkSpace = World.ToChunkSpace(Model.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null)
            {
                AddIfNecessary(Model, chunk);
            }    
        }

        private void AddIfNecessary(CachedVertexData Model, Chunk Object)
        {
            lock (_lock)
            {
                if (!Object.Disposed && Object.Initialized && (!_chunksAdded.ContainsKey(Model) || _chunksAdded[Model] != Object))
                {
                    Object.AddStaticElement(Model.VertexData);
                    if (_chunksAdded.ContainsKey(Model))
                        _chunksAdded[Model] = Object;
                    else
                        _chunksAdded.Add(Model, Object);
                }
            }
        }

        private void OnChunkDisposed(Chunk Object)
        {
            KeyValuePair<CachedVertexData, Chunk>[] dict;
            lock (_lock)
            {
                dict = _chunksAdded.ToArray();
            }
            foreach (var pair in dict)
            {
                if (pair.Value == Object)
                    _chunksAdded[pair.Key] = null;
            }          
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach(var pair in _chunksAdded)
                {
                    if (pair.Value != null)
                        pair.Value.RemoveStaticElement(pair.Key.VertexData);
                }
                _chunksAdded.Clear();
            }
            Structure.Dispose();
        }
    }
}