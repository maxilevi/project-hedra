/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 07:36 p.m.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using System.Numerics;
using Hedra.Engine.IO;
using Hedra.Game;
using Hedra.Numerics;
using Timer = Hedra.Core.Timer;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkLoader
    {
        public event OnChunkEvent OnChunkReady;
        public bool ShouldUpdateFog { get; set; } = true;
        public bool Enabled { get; set; }
        public Vector2 Offset { get; private set; }
        public int ActiveChunks => (int) _activeChunks;
        public float MinFog { get; private set; }
        public float MaxFog { get; private set; }
        private readonly object Lock = new object();
        private readonly List<ChunkWatcher> _chunkWatchers;
        private readonly List<Vector3> _candidates;
        private readonly ClosestComparer _closest;
        private readonly AutoResetEvent _resetEvent;
        private readonly Thread _mainThread;
        private readonly IPlayer _player;
        private float _targetMin = 1;
        private float _targetMax = 1;
        private float _activeChunks;
        private float _targetActivechunks;
        private int _lastRadius;

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

        public void UpdateFog(bool Force = false)
        {
            MaxFog = (float)Math.Max(1, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks)-1) * 2f);
            MinFog = MaxFog - 16;

            if (Math.Abs(_activeChunks - _targetActivechunks) > .05f || Force)
            {
                SkyManager.FogManager.UpdateFogSettings(MinFog, MaxFog);
            }
        }

        public void Update()
        {
            _activeChunks = Mathf.Lerp(_activeChunks, _targetActivechunks, Time.IndependentDeltaTime * .5f);
            this.UpdateFog();
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
            var watchers = Watchers;
            for (var i = watchers.Length - 1; i > -1; --i)
            {
                watchers[i]?.Update();
                if (watchers[i]?.Disposed ?? false)
                {
                    lock (Lock)
                        _chunkWatchers.Remove(watchers[i]);
                }
            }
        }

        private void CreateIfNecessary()
        {
            Offset = World.ToChunkSpace(_player.Position);
            if (World.IsGenerated && Enabled)
            {
                var hadChanges = false;
                var stopCounting = false;
                var newTarget = 0;
                BuildCandidates(_candidates);
                for (var i = 0; i < _candidates.Count; i++)
                {
                    var obj = World.GetChunkByOffset(_candidates[i].Xz());
                    if (obj == null || !obj.BuildedWithStructures || obj.Disposed) stopCounting = true;
                    if (!stopCounting) newTarget++;
                    if (obj != null) continue;
                    var chunk = new Chunk((int) _candidates[i].X, (int) _candidates[i].Z);
                    World.AddChunk(chunk);
                    var watcher = new ChunkWatcher(chunk);
                    watcher.OnChunkReady += O => OnChunkReady?.Invoke(O);
                    lock (Lock)
                        _chunkWatchers.Add(watcher);
                }

                _targetActivechunks = newTarget;
            }
        }

        private void BuildCandidates(List<Vector3> Candidates)
        {
            if(World.ToChunkSpace(_closest.Position) == World.ToChunkSpace(_player.Position) && _lastRadius == GameSettings.ChunkLoaderRadius) return;
            Candidates.Clear();
            var radius = (int) (GameSettings.ChunkLoaderRadius * .5f);
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
                for (var i = _chunkWatchers.Count - 1; i > -1; i--)
                {
                    _chunkWatchers[i]?.Kill();
                }
            }
            _targetActivechunks = 0;
            _activeChunks = 0;
            lock (Lock)
                _chunkWatchers.Clear();
        }

        private ChunkWatcher[] Watchers
        {
            get
            {
                lock (Lock)
                    return _chunkWatchers.ToArray();
            }
        }

        public int WatcherCount
        {
            get
            {
                lock (Lock)
                    return _chunkWatchers.Count;
            }   
        }
    }
}