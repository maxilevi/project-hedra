using System;
using System.Threading;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;

namespace Hedra.Engine.Generation
{
    public class WorldBuilder
    {
        private readonly StructuresBuilder _structuresBuilder;
        private readonly BlockBuilder _blockBuilder;
        private readonly MeshBuilder _meshBuilder;
        private readonly SharedWorkerPool _pool;

        public WorldBuilder()
        {
            _pool = new SharedWorkerPool(Environment.ProcessorCount * 2);
            _meshBuilder = new MeshBuilder(_pool);
            _blockBuilder = new BlockBuilder(_pool);
            _structuresBuilder = new StructuresBuilder(_pool);
        }

        private void HandleMaxWorkers()
        {
            var minMesh = Environment.ProcessorCount;
            var workerCount = Environment.ProcessorCount * 2;
            if (_meshBuilder.Count > 0)
            {
                workerCount -= minMesh;
                _pool.SetMaxWorkers(QueueType.Meshing, minMesh);
            }
            else
            {
                _pool.SetMaxWorkers(QueueType.Meshing, 0);
            }

            if (_blockBuilder.Count == 0)
            {
                _pool.SetMaxWorkers(QueueType.Structures, workerCount);
            }
            else if(_structuresBuilder.Count == 0)
            {
                _pool.SetMaxWorkers(QueueType.Blocks, workerCount); 
            }
            else
            {
                _pool.SetMaxWorkers(QueueType.Blocks, Math.Max(1, workerCount / 2)); 
                _pool.SetMaxWorkers(QueueType.Structures, Math.Max(1, workerCount / 2)); 
            }
        }

        public int MeshThreads => _pool.GetMaxWorkers(QueueType.Meshing);
        public int BlockThreads => _pool.GetMaxWorkers(QueueType.Blocks);
        public int StructureThreads => _pool.GetMaxWorkers(QueueType.Structures);
        
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
            HandleMaxWorkers();
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