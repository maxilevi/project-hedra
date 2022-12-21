using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;
using Hedra.Engine.Management;

namespace Hedra.Engine.Generation
{
    public class WorldBuilder
    {
        private readonly BlockBuilder _blockBuilder;
        private readonly int _maxThreads;
        private readonly MeshBuilder _meshBuilder;
        private readonly SharedWorkerPool _pool;
        private readonly StructuresBuilder _structuresBuilder;

        public WorldBuilder()
        {
            _maxThreads = (int)(Environment.ProcessorCount / 2);
            _pool = new SharedWorkerPool(_maxThreads);
            _meshBuilder = new MeshBuilder(_pool);
            _blockBuilder = new BlockBuilder(_pool);
            _structuresBuilder = new StructuresBuilder(_pool);
            RoutineManager.StartRoutine(DoUpdate);
        }

        public int MeshThreads => _pool.GetMaxWorkers(QueueType.Meshing);
        public int BlockThreads => _pool.GetMaxWorkers(QueueType.Blocks);
        public int StructureThreads => _pool.GetMaxWorkers(QueueType.Structures);

        public int AverageBuildTime => _meshBuilder.AverageWorkTime;

        public int AverageBlockTime => _blockBuilder.AverageWorkTime;

        public int AverageStructureTime => _structuresBuilder.AverageWorkTime;

        public int MeshQueueCount => _meshBuilder.Count;

        public int BlockQueueCount => _blockBuilder.Count;

        public int StructureQueueCount => _structuresBuilder.Count;

        private void HandleMaxWorkers()
        {
            var minMesh = _maxThreads / 2;
            var workerCount = _maxThreads;
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
                _pool.SetMaxWorkers(QueueType.Meshing, _maxThreads);
                _pool.SetMaxWorkers(QueueType.Blocks, 0);
            }
            else
            {
                _pool.SetMaxWorkers(QueueType.Blocks, workerCount);
            }
            
        }

        public void Process(Chunk Chunk, ChunkQueueType Type)
        {
            if (Type == ChunkQueueType.Mesh)
            {
                _meshBuilder.Add(Chunk);
            }
            else
            {
                _blockBuilder.Add(Chunk);
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

        }
        
        private IEnumerator DoUpdate()
        {
            while (Program.GameWindow.Exists)
            {
                HandleMaxWorkers();
                _meshBuilder.Update();
                _blockBuilder.Update();
                //_structuresBuilder.Update();
                yield return null;
            }
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
    }
}