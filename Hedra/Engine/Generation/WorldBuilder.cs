using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;

namespace Hedra.Engine.Generation
{
    public class WorldBuilder
    {
        private readonly StructuresBuilder _structuresBuilder;
        private readonly BlockBuilder _blockBuilder;
        private readonly MeshBuilder _meshBuilder;

        public WorldBuilder()
        {
            var pool = new SharedWorkerPool(3);
            _meshBuilder = new MeshBuilder(pool);
            _blockBuilder = new BlockBuilder(pool);
            _structuresBuilder = new StructuresBuilder(pool);
        }

        public void Process(Chunk Chunk, ChunkQueueType Type)
        {
            if (Type == ChunkQueueType.Mesh)
            {
                _meshBuilder.Add(Chunk);
            }
            else
            {
                if(!Chunk.Landscape.BlocksDefined)
                    _blockBuilder.Add(Chunk);
                else
                    _structuresBuilder.Add(Chunk);
            }
        }

        public void Remove(Chunk Chunk)
        {
            _meshBuilder.Remove(Chunk);
            _blockBuilder.Remove(Chunk);
            _structuresBuilder.Remove(Chunk);
        }
        
        public void Update()
        {
            _meshBuilder.Update();
            _blockBuilder.Update();
            _structuresBuilder.Update();
        }

        public void Discard()
        {
            _meshBuilder.Discard();
            _blockBuilder.Discard();
            _structuresBuilder.Discard();
        }

        public void ResetGenerationProfile()
        {
            _blockBuilder.ResetProfile();
            _structuresBuilder.ResetProfile();
        }

        public void ResetMeshProfile()
        {
            _meshBuilder.ResetProfile();
        }

        public int AverageBuildTime => _meshBuilder.AverageWorkTime;

        public int AverageBlockTime => _blockBuilder.AverageWorkTime;

        public int AverageStructureTime => _structuresBuilder.AverageWorkTime;

        public int MeshQueueCount => _meshBuilder.Count;

        public int BlockQueueCount => _blockBuilder.Count;
        
        public int StructureQueueCount => _structuresBuilder.Count;
    }
}