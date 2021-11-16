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
        private readonly List<Chunk> _queue;
        private readonly Stopwatch _watch;
        private float _accumTime;
        private bool _discard;
        private int _lastCount;
        private Vector3 _lastSortedPosition;
        private int _workItems;

        protected AbstractBuilder(SharedWorkerPool Pool)
        {
            _pool = Pool;
            _lock = new object();
            _queue = new List<Chunk>();
            _hashQueue = new HashSet<Chunk>();
            _closest = new ChunkComparer();
            _watch = new Stopwatch();
        }

        protected abstract QueueType Type { get; }

        public int AverageWorkTime => (int)(_accumTime / Math.Max(_workItems, 1));
        public int Count => _queue.Count;

        public void Dispose()
        {
        }

        private Chunk GetFirstClosest()
        {
            _closest.Position = GameManager.Player?.Position ?? Vector3.Zero;
            Chunk bestChunk = null;
            var currDist = 10e9;
            for (var i = 0; i < _queue.Count; ++i)
            {
                if (_queue[i] == null) continue;
                var dist = (_queue[i].Position - _closest.Position).LengthSquared();
                if (dist < currDist)
                {
                    bestChunk = _queue[i];
                    currDist = dist;
                }
            }

            return bestChunk;
        }

        public void Update()
        {
            lock (_lock)
            {
                if (_discard)
                {
                    _queue.Clear();
                    _hashQueue.Clear();
                    _discard = false;
                }

                if (_queue.Count > 0)
                {
                    var chunk = GetFirstClosest();
                    if (chunk?.Disposed ?? false)
                    {
                        _queue.Remove(chunk);
                        _hashQueue.Remove(chunk);
                        return;
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
                                _queue.Remove(chunk);
                                _hashQueue.Remove(chunk);
                            }
                        }

                        lock (_lock)
                        {
                            _hashQueue.Remove(chunk);
                        }
                    });
                    if (result) _queue.Remove(chunk);
                }
            }
        }

        protected abstract void Work(Chunk Object);

        public void Add(Chunk Chunk)
        {
            lock (_lock)
            {
                if (Chunk == null || Chunk.Disposed || _hashQueue.Contains(Chunk)) return;
                _queue.Add(Chunk);
                _hashQueue.Add(Chunk);
            }
        }

        public void Remove(Chunk Chunk)
        {
            lock (_lock)
            {
                if (Chunk == null || !_hashQueue.Contains(Chunk)) return;
                _queue.Remove(Chunk);
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