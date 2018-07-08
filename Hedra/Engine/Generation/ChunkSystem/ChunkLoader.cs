/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 07:36 p.m.
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    internal class ChunkLoader
    {
        public bool Enabled { get; set; }
        public Vector2 Offset { get; private set; }
        public int ActiveChunks => (int) _activeChunks;
        public float MinFog { get; private set; }
        public float MaxFog { get; private set; }
        private readonly List<ChunkWatcher> _chunkWatchers;
        private readonly Thread _mainThread;
        private readonly LocalPlayer _player;
        private float _targetMin = 1;
        private float _targetMax = 1;
        private float _activeChunks;
        private float _targetActivechunks;

        public ChunkLoader(LocalPlayer Player)
        {
            _player = Player;
            Enabled = true;
            _chunkWatchers = new List<ChunkWatcher>();
            _mainThread = new Thread(this.Update)
            {
                IsBackground = true
            };
            _mainThread.Start();
        }

        public void UpdateFog()
        {
            MaxFog = (float) Math.Max(1, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks) - 2) * 2.00f);
            MinFog = (float) Math.Max(0, Chunk.Width / Chunk.BlockSize * (Math.Sqrt(_activeChunks) - 3) * 2.00f);

            //if (Math.Abs(MaxFog - _targetMax) > 0.005f || Math.Abs(MinFog - _targetMin) > 0.005f)
            {
                if (Math.Abs(_targetMax - MaxFog) > .005f)
                {
                    Executer.ExecuteOnMainThread(delegate
                    {
                        SkyManager.FogManager.UpdateFogSettings(MinFog, MaxFog);
                    });
                }
            }
        }

        private void Update()
        {
            while (Program.GameWindow.Exists)
            {
                if (!World.IsGenerated || !Enabled) continue;

                Offset = World.ToChunkSpace(_player.BlockPosition);
                var radius = (int) (GameSettings.ChunkLoaderRadius * .4f);
                for (var x = -radius; x < radius; x++)
                {
                    for (var z = -radius; z < radius; z++)
                    {
                        if (World.GetChunkByOffset(
                                Offset + new Vector2(x, z) * new Vector2(Chunk.Width, Chunk.Width)) !=
                            null) continue;
                        Vector2 chunkPos = Offset + new Vector2(x, z) * new Vector2(Chunk.Width, Chunk.Width);
                        var chunk = new Chunk((int) chunkPos.X, (int) chunkPos.Y);
                        World.AddChunk(chunk);
                        _chunkWatchers.Add(new ChunkWatcher(chunk));
                    }
                }

                _targetActivechunks = 0;
                for (var i = _chunkWatchers.Count - 1; i > -1; i--)
                {
                    _chunkWatchers[i].Update();
                    if (_chunkWatchers[i].IsHealthy) _targetActivechunks++;
                    if (_chunkWatchers[i].Disposed) _chunkWatchers.RemoveAt(i);
                }
                _activeChunks = Mathf.Lerp(_activeChunks, _targetActivechunks,  Time.unScaledDeltaTime * 4f);
                if (float.IsInfinity(_activeChunks)) _activeChunks = _targetActivechunks;
                this.UpdateFog();
            }
        }
    }
}