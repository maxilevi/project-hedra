using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Generation.ChunkSystem.Builders;
using Hedra.Engine.Management;
using Hedra.Game;
using Hedra.Engine.IO;

namespace Hedra.Engine.Generation
{
    public class WorldBuilder
    {
        private readonly BlockBuilder _blockBuilder;
        private readonly int _maxThreads;
        private readonly MeshBuilder _meshBuilder;
        private readonly SharedWorkerPool _pool;
        private readonly StructuresBuilder _structuresBuilder;
        private readonly GenerationJobQueue _geometry;
        private readonly AutoResetEvent _workAvailable = new(false);
        private readonly Thread _scheduler;

        public WorldBuilder()
        {
            _maxThreads = Math.Max(2, Environment.ProcessorCount / 2);
            _pool = new SharedWorkerPool(_maxThreads);
            _meshBuilder = new MeshBuilder(_pool);
            _blockBuilder = new BlockBuilder(_pool);
            _structuresBuilder = new StructuresBuilder(_pool);
            _geometry = new GenerationJobQueue(_pool);
            _pool.WorkerAvailable += () => _workAvailable.Set();
            _scheduler = new Thread(Schedule)
            {
                IsBackground = true,
                Name = "World generation scheduler",
                Priority = ThreadPriority.BelowNormal
            };
            if (!GameSettings.TestingMode) _scheduler.Start();
        }

        public int MeshThreads => _pool.GetMaxWorkers(QueueType.Meshing);
        public int BlockThreads => _pool.GetMaxWorkers(QueueType.Blocks);
        public int StructureThreads => _pool.GetMaxWorkers(QueueType.Structures);
        public int PendingStructureBuilds => _geometry.PendingCount;

        public int AverageBuildTime => _meshBuilder.AverageWorkTime;

        public int AverageBlockTime => _blockBuilder.AverageWorkTime;

        public int AverageStructureTime => _structuresBuilder.AverageWorkTime;

        public int MeshQueueCount => _meshBuilder.Count;

        public int BlockQueueCount => _blockBuilder.Count;

        public int StructureQueueCount => _geometry.Count;

        private void HandleMaxWorkers()
        {
            var geometryThreads = _geometry.Count > 0 ? Math.Min(2, _maxThreads - 1) : 0;
            _pool.SetMaxWorkers(QueueType.Structures, geometryThreads);
            var workerCount = _maxThreads - geometryThreads;
            var minMesh = Math.Max(1, workerCount / 2);
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
                _pool.SetMaxWorkers(QueueType.Meshing, _maxThreads - geometryThreads);
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
            _workAvailable.Set();
        }

        public void Remove(Chunk Chunk)
        {
            _meshBuilder.Remove(Chunk);
            _blockBuilder.Remove(Chunk);
            _structuresBuilder.Remove(Chunk);
        }

        public void ProcessStructure(Action Build)
        {
            _geometry.Enqueue(Build);
            _workAvailable.Set();
        }

        public void Update()
        {

        }
        
        private void Schedule()
        {
            while (Program.GameWindow?.Exists ?? false)
            {
                _workAvailable.WaitOne(100);
                try
                {
                    bool dispatched;
                    do
                    {
                        HandleMaxWorkers();
                        dispatched = _geometry.TryDispatch();
                        dispatched |= _meshBuilder.TryDispatch();
                        dispatched |= _blockBuilder.TryDispatch();
                    } while (dispatched);
                }
                catch (Exception e)
                {
                    Log.WriteLine($"World generation scheduler failed: {e}");
                }
            }
        }

        public void Discard()
        {
            _meshBuilder.Discard();
            _blockBuilder.Discard();
            _structuresBuilder.Discard();
            _geometry.Discard();
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
