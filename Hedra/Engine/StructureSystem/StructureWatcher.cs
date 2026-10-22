using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public class StructureWatcher
    {
        private readonly InstanceDataChunkWatcher _instanceDataWatcher;
        private readonly CachedVertexDataChunkWatcher _vertexDataWatcher;

        public StructureWatcher(CollidableStructure Structure)
        {
            this.Structure = Structure;
            _vertexDataWatcher = new CachedVertexDataChunkWatcher(() => Structure?.Models);
            _instanceDataWatcher = new InstanceDataChunkWatcher(() => Structure?.Instances);
            Structure.ModelAdded += ModelAdded;
            Structure.InstanceAdded += InstanceAdded;
        }

        public CollidableStructure Structure { get; }

        private void InstanceAdded(InstanceData Instance)
        {
            var chunkSpace = World.ToChunkSpace(Instance.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null) _instanceDataWatcher.OnValueAdded(chunk, Instance);
        }

        private void ModelAdded(CachedVertexData Model)
        {
            var chunkSpace = World.ToChunkSpace(Model.Position);
            var chunk = World.GetChunkByOffset(chunkSpace);
            if (chunk != null) _vertexDataWatcher.OnValueAdded(chunk, Model);
        }

        public void Dispose()
        {
            Structure.Dispose();
            Structure.ModelAdded -= ModelAdded;
            Structure.InstanceAdded -= InstanceAdded;
            _instanceDataWatcher.Dispose();
            _vertexDataWatcher.Dispose();
        }
    }
}
