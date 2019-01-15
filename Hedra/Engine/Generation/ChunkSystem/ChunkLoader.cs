/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 07:36 p.m.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;
using Timer = Hedra.Engine.Management.Timer;

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
        private readonly List<ChunkWatcher> _chunkWatchers;
        private readonly List<Vector3> _candidates;
        private readonly ClosestComparer _closest;
        private readonly Thread _mainThread;
        private readonly IPlayer _player;
        private float _targetMin = 1;
        private float _targetMax = 1;
        private float _activeChunks;
        private float _targetActivechunks;

        public ChunkLoader(IPlayer Player)
        {
            Enabled = true;
            _player = Player;
            _chunkWatchers = new List<ChunkWatcher>();
            _candidates = new List<Vector3>();
            _closest = new ClosestComparer();
            CoroutineManager.StartCoroutine(this.CreateChunksCoroutine);
            CoroutineManager.StartCoroutine(this.UpdateChunkCoroutine);
            OnChunkReady += World.MarkChunkReady;
        }

        public void UpdateFog(bool Force = false)
        {
            MaxFog = (float)Math.Max(1, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks) - 2) * 2.00f);
            MinFog = (float)Math.Max(0, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks) - 4) * 2.00f);

            if (Math.Abs(_activeChunks - _targetActivechunks) > .05f || Force)
            {
                SkyManager.FogManager.UpdateFogSettings(MinFog, MaxFog);
            }
        }

        public void Update()
        {
            _activeChunks = Mathf.Lerp(_activeChunks, _targetActivechunks, Time.IndependantDeltaTime * .5f);
            this.UpdateFog();
        }

        private IEnumerator UpdateChunkCoroutine()
        {
            var updateTimer = new Timer(0.05f);
            while (GameManager.Exists)
            {
                if(!updateTimer.Tick()) continue;
                for (var i = _chunkWatchers.Count - 1; i > -1; i--)
                {
                    _chunkWatchers[i].Update();
                    if (_chunkWatchers[i].Disposed) _chunkWatchers.RemoveAt(i);
                }
                yield return null;
            }
        }

        private IEnumerator CreateChunksCoroutine()
        {
            var creationTimer = new Timer(0.1f);
            while (GameManager.Exists)
            {
                Offset = World.ToChunkSpace(_player.BlockPosition);
                if (World.IsGenerated && Enabled && creationTimer.Tick())
                {
                    var hadChanges = false;
                    var stopCounting = false;
                    var newTarget = 0;
                    BuildCandidates(_candidates);
                    for (var i = 0; i < _candidates.Count; i++)
                    {
                        var obj = World.GetChunkByOffset(_candidates[i].Xz);
                        if (obj == null || !obj.BuildedWithStructures || obj.Disposed) stopCounting = true;
                        if (!stopCounting) newTarget++;
                        if (obj != null) continue;
                        var chunk = new Chunk((int) _candidates[i].X, (int) _candidates[i].Z);
                        World.AddChunk(chunk);
                        var watcher = new ChunkWatcher(chunk);
                        watcher.OnChunkReady += O => OnChunkReady?.Invoke(O);
                        _chunkWatchers.Add(watcher);
                    }

                    _targetActivechunks = newTarget;
                }

                yield return null;
            }
        }

        private void BuildCandidates(List<Vector3> Candidates)
        {
            Candidates.Clear();
            var radius = (int) (GameSettings.ChunkLoaderRadius * .5f);
            for (var x = -radius; x < radius; x++)
            {
                for (var z = -radius; z < radius; z++)
                {
                    var radiusOffset = new Vector2(x, z);
                    if (radiusOffset.LengthSquared > radius * radius) continue;
                    var chunkPos = Offset + radiusOffset * new Vector2(Chunk.Width, Chunk.Width);
                    Candidates.Add(chunkPos.ToVector3());
                }
            }
            _closest.Position = _player.Position.Xz.ToVector3();
            Candidates.Sort(_closest);
        }
        
        public void Reset()
        {
            for (var i = _chunkWatchers.Count - 1; i > -1; i--)
            {
                _chunkWatchers[i].Kill();
            }
            _targetActivechunks = 0;
            _activeChunks = 0;
        }

        public int WatcherCount => _chunkWatchers.Count;
    }
}