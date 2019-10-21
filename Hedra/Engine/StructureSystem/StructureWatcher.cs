using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using System.Numerics;

namespace Hedra.Engine.StructureSystem
{
    public class StructureWatcher
    {
        public CollidableStructure Structure { get; }
        private readonly CachedVertexDataChunkWatcher _vertexDataWatcher;
        private readonly InstanceDataChunkWatcher _instanceDataWatcher;

        public StructureWatcher(CollidableStructure Structure)
        {
            this.Structure = Structure;
            _vertexDataWatcher = new CachedVertexDataChunkWatcher(() => Structure.Models);
            _instanceDataWatcher = new InstanceDataChunkWatcher(() => Structure.Instances);
            Structure.ModelAdded += this.ModelAdded;
            Structure.InstanceAdded += this.InstanceAdded;
        }
        
        private void InstanceAdded(InstanceData Instance)
        {
            var chunkSpace = World.ToChunkSpace(Instance.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null)
            {
                _instanceDataWatcher.OnChunkReady(chunk);
            }    
        }

        private void ModelAdded(CachedVertexData Model)
        {
            var chunkSpace = World.ToChunkSpace(Model.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null)
            {
                _vertexDataWatcher.OnChunkReady(chunk);
            }    
        }

        public void Dispose()
        {
            Structure.Dispose();
            Structure.ModelAdded -= this.ModelAdded;
            Structure.InstanceAdded -= this.InstanceAdded;
            _instanceDataWatcher.Dispose();
            _vertexDataWatcher.Dispose();
        }
    }
}