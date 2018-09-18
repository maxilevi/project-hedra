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
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

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
            CoroutineManager.StartCoroutine(this.UpdateCoroutine);
            OnChunkReady += World.MarkChunkReady;
        }

        public void UpdateFog(bool Force = false)
        {
            MaxFog = (float)Math.Max(1, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks) - 2) * 2.00f);
            MinFog = (float)Math.Max(0, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks) - 3) * 2.00f);

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

        private IEnumerator UpdateCoroutine()
        {
            while (GameManager.Exists)
            {

                Offset = World.ToChunkSpace(_player.BlockPosition);
                if (World.IsGenerated && Enabled)
                {
                    var radius = (int) (GameSettings.ChunkLoaderRadius * .5f);
                    var hadChanges = false;
                    for (var x = -radius; x < radius; x++)
                    {
                        for (var z = -radius; z < radius; z++)
                        {
                            yield return null;
                            var radiusOffset = new Vector2(x, z);
                            if (radiusOffset.LengthSquared > radius * radius) continue;
                            var chunkPos = Offset + radiusOffset * new Vector2(Chunk.Width, Chunk.Width);
                            if (World.GetChunkByOffset(chunkPos) != null) continue;
                            var chunk = new Chunk((int) chunkPos.X, (int) chunkPos.Y);
                            World.AddChunk(chunk);
                            var watcher = new ChunkWatcher(chunk);
                            watcher.OnChunkReady += (O) => OnChunkReady?.Invoke(O);
                            _chunkWatchers.Add(watcher);
                        }
                    }
                }
                var newTarget = 0;
                for (var i = _chunkWatchers.Count - 1; i > -1; i--)
                {
                    _chunkWatchers[i].Update();
                    if (_chunkWatchers[i].IsHealthy) newTarget++;
                    if (_chunkWatchers[i].IsHealthy) yield return null;
                    if (_chunkWatchers[i].Disposed) _chunkWatchers.RemoveAt(i);
                }
                _targetActivechunks = newTarget;
                yield return null;
            }
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
    }
}