/*
 * Author: Zaphyk
 * Date: 18/02/2016
 * Time: 05:11 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Game;

namespace Hedra.Engine.Generation.ChunkSystem.Builders
{
    public abstract class AbstractBuilder : ICountable, IDisposable
    {
        private readonly ChunkComparer _closest;
        private readonly HashSet<Chunk> _hashQueue;
        private readonly object _lock;
        private readonly SharedWorkerPool _pool;
        private readonly PriorityQueue<Chunk, float> _queue;
        private long _accumTicks;
        private long _workItems;

        protected AbstractBuilder(SharedWorkerPool Pool)
        {
            _pool = Pool;
            _lock = new object();
            _queue = new PriorityQueue<Chunk, float>();
            _hashQueue = new HashSet<Chunk>();
            _closest = new ChunkComparer();
        }

        protected abstract QueueType Type { get; }

        public int AverageWorkTime => (int)(Interlocked.Read(ref _accumTicks) * 1000.0 / Stopwatch.Frequency
                                            / Math.Max(Interlocked.Read(ref _workItems), 1));
        public int Count { get { lock (_lock) return _queue.Count; } }

        public void Dispose()
        {
        }

        private Chunk GetFirstClosest()
        {
            while (_queue.Count > 0)
            {
                var chunk = _queue.Dequeue();
                if (_hashQueue.Contains(chunk)) return chunk;
            }
            return null;
        }

        public void Update()
        {
            while (TryDispatch()) { }
        }

        public bool TryDispatch()
        {
            Chunk chunk;
            lock (_lock)
            {
                if (_queue.Count == 0) return false;
                chunk = GetFirstClosest();
                if (chunk == null) return false;
                if (chunk?.Disposed ?? false)
                {
                    _hashQueue.Remove(chunk);
                    return true;
                }
            }

            var result = _pool.Work(this, Type, delegate
            {
                try
                {
                    if (chunk?.Disposed ?? true) return;
                    var start = Stopwatch.GetTimestamp();
                    Work(chunk);
                    Interlocked.Add(ref _accumTicks, Stopwatch.GetTimestamp() - start);
                    Interlocked.Increment(ref _workItems);
                }
                catch (Exception e)
                {
                    lock (_lock)
                    {
                        Log.WriteLine($"Failed to do job: {Environment.NewLine}{e}");
                        _hashQueue.Remove(chunk);
                    }
                }

                lock (_lock)
                {
                    _hashQueue.Remove(chunk);
                }
            });
            if (!result)
            {
                lock (_lock)
                {
                    _queue.Enqueue(chunk, Vector3.Distance(GameManager.Player?.Position ?? Vector3.Zero, chunk.Position));
                }
            }
            return result;
        }

        protected abstract void Work(Chunk Object);

        public void Add(Chunk Chunk)
        {
            lock (_lock)
            {
                if (Chunk == null || Chunk.Disposed || _hashQueue.Contains(Chunk)) return;
                _queue.Enqueue(Chunk, Vector3.Distance(GameManager.Player?.Position ?? Vector3.Zero, Chunk.Position));
                _hashQueue.Add(Chunk);
            }
        }

        public void Remove(Chunk Chunk)
        {
            lock (_lock)
            {
                if (Chunk == null || !_hashQueue.Contains(Chunk)) return;
                //_queue.Remove(Chunk);
                _hashQueue.Remove(Chunk);
            }
        }

        public void Discard()
        {
            lock (_lock)
            {
                _queue.Clear();
                _hashQueue.Clear();
            }
            ResetProfile();
        }

        public void ResetProfile()
        {
            Interlocked.Exchange(ref _accumTicks, 0);
            Interlocked.Exchange(ref _workItems, 0);
        }
    }
}
