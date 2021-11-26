/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 07:36 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Player;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkLoader
    {
        private readonly List<Vector3> _candidates;
        private readonly List<ChunkWatcher> _chunkWatchers;
        private readonly ClosestComparer _closest;
        private readonly Thread _mainThread;
        private readonly IPlayer _player;
        private readonly AutoResetEvent _resetEvent;
        private readonly object Lock = new object();
        private float _activeChunks;
        private int _lastRadius;
        private float _targetActivechunks;
        private float _targetMax = 1;
        private float _targetMin = 1;

        public ChunkLoader(IPlayer Player)
        {
            Enabled = true;
            _player = Player;
            _mainThread = new Thread(UpdateLoop)
            {
                Priority = ThreadPriority.BelowNormal
            };
            _chunkWatchers = new List<ChunkWatcher>();
            _candidates = new List<Vector3>();
            _closest = new ClosestComparer();
            _resetEvent = new AutoResetEvent(false);
            _mainThread.Start();
            OnChunkReady += World.MarkChunkReady;
        }

        public bool ShouldUpdateFog { get; set; } = true;
        public bool Enabled { get; set; }
        public Vector2 Offset { get; private set; }
        public int ActiveChunks => (int)_activeChunks;
        public float MinFog { get; private set; }
        public float MaxFog { get; private set; }

        public int WatcherCount
        {
            get
            {
                lock (Lock)
                {
                    return _chunkWatchers.Count;
                }
            }
        }

        public event OnChunkEvent OnChunkReady;

        public void UpdateFog(bool Force = false)
        {
            MaxFog = Math.Max(1, _activeChunks) * Chunk.Width;
            MinFog = MaxFog - Chunk.Width * 4;

            if (Math.Abs(_activeChunks - _targetActivechunks) > .05f || Force)
                SkyManager.FogManager.UpdateFogSettings(MinFog, MaxFog);
        }

        public void Update()
        {
            _activeChunks = Mathf.Lerp(_activeChunks, _targetActivechunks - (_activeChunks > 4 ? 0.5f : 1.5f), Time.IndependentDeltaTime * .5f);
            UpdateFog();
        }

        public void Dispatch()
        {
            _resetEvent.Set();
        }

        private void UpdateLoop()
        {
            Time.RegisterThread();
            while (GameManager.Exists)
            {
                _resetEvent.WaitOne();
                UpdateWatchers();
                CreateIfNecessary();
            }
        }

        private void UpdateWatchers()
        {
            lock (Lock)
            {
                var watchers = _chunkWatchers;
                for (var i = watchers.Count-1; i > -1; --i)
                {
                    watchers[i]?.Update();
                    if (watchers[i]?.Disposed ?? false)
                    {
                        lock (Lock)
                        {
                            _chunkWatchers.Remove(watchers[i]);
                        }
                    }
                }
            }
        }

        private void CreateIfNecessary()
        {
            Offset = World.ToChunkSpace(_player.Position);
            if (World.IsGenerated && Enabled)
            {
                BuildCandidates(_candidates);
                for (var i = 0; i < _candidates.Count; i++)
                {
                    var obj = World.GetChunkByOffset(_candidates[i].Xz());
                    if (obj != null) continue;
                    CreateChunk(_candidates[i].Xz());
                }


                var directions = new[]
                {
                    new Vector2(Chunk.Width, 0),
                    new Vector2(0, Chunk.Width),
                    new Vector2(-Chunk.Width, 0),
                    new Vector2(0, -Chunk.Width)
                };
                var minTarget = int.MaxValue;
                foreach (var direction in directions)
                {
                    var newTarget = 0;
                    for (var i = 0; i < GameSettings.ChunkLoaderRadius; ++i)
                    {
                        var chunk = World.GetChunkByOffset(new Vector2(Offset.X, Offset.Y) + direction * i);
                        if (chunk == null || !chunk.BuildedWithStructures || chunk.Disposed) break;
                        newTarget += 1;
                    }

                    minTarget = Math.Min(minTarget, newTarget);
                }

                _targetActivechunks = minTarget;
            }
        }

        private void CreateChunk(Vector2 NewOffset)
        {
            var chunk = new Chunk((int)NewOffset.X, (int)NewOffset.Y);
            World.AddChunk(chunk);
            var watcher = new ChunkWatcher(chunk);
            watcher.OnChunkReady += O => OnChunkReady?.Invoke(O);
            lock (Lock)
            {
                _chunkWatchers.Add(watcher);
            }
        }

        private void BuildCandidates(List<Vector3> Candidates)
        {
            if (World.ToChunkSpace(_closest.Position) == World.ToChunkSpace(_player.Position) && _lastRadius == GameSettings.ChunkLoaderRadius) return;
            Candidates.Clear();
            var radius = GameSettings.ChunkLoaderRadius >> 1;
            for (var x = -radius; x < radius; x++)
            {
                for (var z = -radius; z < radius; z++)
                {
                    var radiusOffset = new Vector2(x, z);
                    if (radiusOffset.LengthSquared() > radius * radius) continue;
                    var chunkPos = Offset + radiusOffset * new Vector2(Chunk.Width, Chunk.Width);
                    Candidates.Add(chunkPos.ToVector3());
                }
            }

            _lastRadius = GameSettings.ChunkLoaderRadius;
            _closest.Position = _player.Position.Xz().ToVector3();
            Candidates.Sort(_closest);
        }

        public void Reset()
        {
            lock (Lock)
            {
                for (var i = _chunkWatchers.Count - 1; i > -1; i--) _chunkWatchers[i]?.Kill();
            }

            _targetActivechunks = 0;
            _activeChunks = 0;
            lock (Lock)
            {
                _chunkWatchers.Clear();
            }
        }
    }
}