using System;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public sealed class CachedVertexDataChunkWatcher : ChunkWatcher<CachedVertexData>
    {
        private readonly Func<CachedVertexData[]> _lambda;

        public CachedVertexDataChunkWatcher(Func<CachedVertexData[]> Lambda)
        {
            _lambda = Lambda;
        }

        protected override void Add(Chunk Object, CachedVertexData Value)
        {
            Object.AddStaticElement(Value.VertexData);
        }

        protected override void Delete(Chunk Object, CachedVertexData Value)
        {
            Object.RemoveStaticElement(Value.VertexData);
        }

        protected override CachedVertexData[] Get()
        {
            return _lambda?.Invoke();
        }
    }
}