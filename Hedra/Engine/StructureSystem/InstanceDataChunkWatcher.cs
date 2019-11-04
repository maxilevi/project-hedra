using System;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public sealed class InstanceDataChunkWatcher : ChunkWatcher<InstanceData>
    {
        private readonly Func<InstanceData[]> _lambda;
        
        public InstanceDataChunkWatcher(Func<InstanceData[]> Lambda)
        {
            _lambda = Lambda;
        }

        protected override void Add(Chunk Object, InstanceData Value)
        {
            Object.AddInstance(Value);
        }

        protected override void Delete(Chunk Object, InstanceData Value)
        {
            Object.RemoveInstance(Value);
        }

        protected override InstanceData[] Get()
        {
            return _lambda?.Invoke();
        }
    }
}