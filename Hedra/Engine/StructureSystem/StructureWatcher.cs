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
        private readonly Dictionary<CachedVertexData, Chunk> _chunksAdded;

        public StructureWatcher(CollidableStructure Structure)
        {
            this.Structure = Structure;
            _chunksAdded = new Dictionary<CachedVertexData, Chunk>();
            World.OnChunkReady += this.OnChunkReady;
            World.OnChunkDisposed += this.OnChunkDisposed;
        }

        private void OnChunkReady(Chunk Object)
        {
            var models = Structure.Models;
            for (var i = 0; i < models.Length; i++)
            {
                var chunkSpace = World.ToChunkSpace(Structure.Position);
                if (Object.OffsetX == (int) chunkSpace.X && Object.OffsetZ == (int) chunkSpace.Y)
                {
                    if (!_chunksAdded.ContainsKey(models[i]) || _chunksAdded[models[i]] != Object)
                    {
                        Object.AddStaticElement(models[i].ToVertexData());
                        if(_chunksAdded.ContainsKey(models[i]))
                            _chunksAdded[models[i]] = Object;
                        else
                            _chunksAdded.Add(models[i], Object);
                    }
                }
            }
        }

        private void OnChunkDisposed(Chunk Object)
        {
            /*var dict = _chunksAdded.ToArray();
            foreach (var pair in dict)
            {
                if (pair.Value == Object)
                    _chunksAdded[pair.Key] = null;
            }*/
        }

        public void Dispose()
        {
            _chunksAdded.Clear();
            Structure.Dispose();
        }
    }
}