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
        private const int MaxChunksPerSecond = 30;
        private readonly ChunkComparer _closest;
        private readonly HashSet<Chunk> _hashQueue;
        private readonly object _lock;
        private readonly SharedWorkerPool _pool;
        private readonly PriorityQueue<Chunk, float> _queue;
        private readonly Stopwatch _watch;
        private float _accumTime;
        private bool _discard;
        private int _lastCount;
        private Vector3 _lastSortedPosition;
        private int _workItems;
        private Stopwatch _timer;
        private int _cooldown;

        protected AbstractBuilder(SharedWorkerPool Pool)
        {
            _pool = Pool;
            _lock = new object();
            _queue = new PriorityQueue<Chunk, float>();
            _hashQueue = new HashSet<Chunk>();
            _closest = new ChunkComparer();
            _watch = new Stopwatch();
            _timer = new Stopwatch();
            _timer.Start();
        }

        protected abstract QueueType Type { get; }

        public int AverageWorkTime => (int)(_accumTime / Math.Max(_workItems, 1));
        public int Count => _queue.Count;

        public void Dispose()
        {
        }

        private Chunk GetFirstClosest()
        {
            Chunk ch = null;
            while (_queue.Count > 0 && (ch == null || !_hashQueue.Contains(ch)))
            {
                ch = _queue.Dequeue();    
            }

            return ch;
        }

        public void Update()
        {
            if (_timer.ElapsedMilliseconds < _cooldown) return;
            
            if (_discard)
            {
                lock (_lock)
                {
                    _queue.Clear();
                    _hashQueue.Clear();
                    _discard = false;
                }
            }

            lock (_lock)
            {
                if (_queue.Count == 0) return;
            }

            Chunk chunk;
            lock (_lock)
            {
                chunk = GetFirstClosest();
                if (chunk?.Disposed ?? false)
                {
                    _hashQueue.Remove(chunk);
                    return;
                }
            }

            var result = _pool.Work(this, Type, delegate
            {
                try
                {
                    if (chunk?.Disposed ?? true) return;
                    StartProfile();
                    Work(chunk);
                    EndProfile();
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
            if (result)
            {
                //_cooldown = 25;//Type == QueueType.Meshing ? 5 : 25;
                //_timer.Restart();
            }
            else
            {
                lock (_lock)
                {
                    _queue.Enqueue(chunk, Vector3.Distance(GameManager.Player?.Position ?? Vector3.Zero, chunk.Position));
                }
            }
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
            _discard = true;
            ResetProfile();
        }

        public void ResetProfile()
        {
            _accumTime = 0;
            _workItems = 0;
        }

        private void StartProfile()
        {
            _watch.Restart();
        }

        private void EndProfile()
        {
            _watch.Stop();
            _accumTime += _watch.ElapsedMilliseconds;
            ++_workItems;
        }
    }
}