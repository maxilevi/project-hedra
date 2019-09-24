/*
 * Author: Zaphyk
 * Date: 18/02/2016
 * Time: 05:11 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Game;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem.Builders
{
    public abstract class AbstractBuilder : ICountable, IDisposable
    {
        public int Count => _queue.Count;
        private readonly Stopwatch _watch;
        private readonly SharedWorkerPool _pool;
        private readonly HashSet<Chunk> _hashQueue;
        private readonly ChunkComparer _closest;
        private readonly List<Chunk> _queue;
        private readonly object _lock;
        private bool _discard;
        private Vector3 _lastSortedPosition;
        private int _lastCount;
        private float _accumTime;
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
                    _closest.Position = GameManager.Player?.Position ?? Vector3.Zero;
                    if ( (_lastSortedPosition - _closest.Position).LengthSquared > Chunk.Width * Chunk.Width || _lastCount != _queue.Count)
                    {
                        _queue.Sort(_closest);
                        _lastSortedPosition = _closest.Position;
                        _lastCount = _queue.Count;
                    }
                    var chunk = _queue[0];
                    if (chunk?.Disposed ?? false)
                    {
                        _queue.Remove(chunk);
                        _hashQueue.Remove(chunk);
                        return;
                    }
                    var result = _pool.Work(this, Type, delegate
                    {
                        var previous = Thread.CurrentThread.Priority;
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                        try
                        {
                            if(chunk?.Disposed ?? true) return;
                            this.StartProfile();
                            this.Work(chunk);
                            this.EndProfile();
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
                        finally
                        {
                            Thread.CurrentThread.Priority = previous;
                        }
                        lock (_lock)
                            _hashQueue.Remove(chunk);
                    }, SleepTime);
                    if (result)
                    {
                        _queue.Remove(chunk);
                    }
                }
            }
        }

        protected abstract QueueType Type { get; }
        protected abstract void Work(Chunk Object);

        protected abstract int SleepTime { get; }

        public int AverageWorkTime => (int) (_accumTime / (float) Math.Max(_workItems, 1));

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

        public void Dispose()
        {

        }
    }
}