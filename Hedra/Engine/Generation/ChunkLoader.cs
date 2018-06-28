/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 07:36 p.m.
 *
 */

using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Scenes;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation.ChunkSystem;


namespace Hedra.Engine.Generation
{
    internal class ChunkLoader
    {
        public Vector2 Offset { get; set; }
        public bool Enabled { get; set; }
        public int ActiveChunks { get; set; }

        private readonly LocalPlayer _player;
        private float _targetMin = 1;
        private float _targetMax = 1;
        public float MinFog { get; private set; }
        public float MaxFog { get; private set; }

        public ChunkLoader(LocalPlayer PlayerRef)
        {
            _player = PlayerRef;
            Enabled = true;
            new Thread(this.LoadChunks).Start();
            new Thread(this.ManageChunks).Start();
            new Thread(this.ManageChunksMesh).Start();
            CoroutineManager.StartCoroutine(this.FogLerpCoroutine);
        }

        public void UpdateFog()
        {
            _targetMax = (float) (Chunk.Width / Chunk.BlockSize * (Math.Sqrt(ActiveChunks)-2) * 2.00f );
            _targetMin = (float) (Chunk.Width / Chunk.BlockSize * (Math.Sqrt(ActiveChunks)-3) * 2.00f );
        }

        private IEnumerator FogLerpCoroutine()
        {
            while (Program.GameWindow.Exists)
            {
                if (MaxFog != _targetMax || MinFog != _targetMin)
                {
                    MaxFog = Mathf.Lerp(MaxFog, _targetMax, (float) Time.unScaledDeltaTime * 8f);
                    MinFog = Mathf.Lerp(MinFog, _targetMin, (float) Time.unScaledDeltaTime * 8f);
                    SkyManager.FogManager.UpdateFogSettings(MinFog, MaxFog);
                }

                yield return null;
                yield return null;
            }
        }

        private float _left;
        private Vector2 _lastPos, _lastPos2;
        private int _prevChunkCount;
        private float _lastRadius;
        private int _previousSeed;
        private int _genCounter;

        private void LoadChunks()
        {
            try
            {
                while (Program.GameWindow.Exists)
                {
                    if (!World.IsGenerated || !Enabled || World.MeshQueue.Discard)
                        goto SLEEP;

                    Offset = World.ToChunkSpace(_player.BlockPosition);
                    var newPos = _player.BlockPosition.Xz;
                    if ( (_lastPos - newPos).LengthSquared > 16*16 || GameSettings.ChunkLoaderRadius != _lastRadius ||
                        GameManager.IsLoading)
                    {
                        for (int x = (int)(-GameSettings.ChunkLoaderRadius * .5f); x < (int)(GameSettings.ChunkLoaderRadius * .5f); x++)
                        {
                            for (int z = (int)(-GameSettings.ChunkLoaderRadius * .5f); z < (int)(GameSettings.ChunkLoaderRadius * .5f); z++)
                            {
                                if (World.GetChunkByOffset( Offset + new Vector2(x, z) * new Vector2(Chunk.Width, Chunk.Width)) ==  null)
                                {
                                    Vector2 chunkPos =
                                        Offset + new Vector2(x, z) * new Vector2(Chunk.Width, Chunk.Width);
                                    var chunk = new Chunk((int) chunkPos.X, (int) chunkPos.Y);
                                    World.AddChunk(chunk);
                                }
                            }
                        }
                        _lastRadius = GameSettings.ChunkLoaderRadius;
                        _lastPos = newPos;
                    }
                    SLEEP:
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
            }
        }

        private void ManageChunksMesh()
        {
            try
            {
                while (Program.GameWindow.Exists)
                {
                    Thread.Sleep(250);

                    if (!World.IsGenerated || !Enabled)
                        continue;

                    Chunk[] Chunks;
                    lock (World.Chunks)
                    {
                        Chunks = World.Chunks.ToArray();
                    }

                    _left += 0.25f;
                    if (_left >= .5f)
                    {
                        ActiveChunks = 0;
                        for (int i = Chunks.Length - 1; i > -1; i--)
                        {
                            if (Chunks[i].Disposed)
                            {
                                continue;
                            }
                            if (Chunks[i].IsGenerated && Chunks[i].BuildedWithStructures)
                            {
                                ActiveChunks++;
                            }

                            if (Chunks[i] != null && Chunks[i].IsGenerated)
                            {
                                var cameraDist = (Chunks[i].Position.Xz - _player.View.CameraPosition.Xz).LengthSquared;

                                if (cameraDist > 288 * 288 && cameraDist < 576 * 576 && GameSettings.Lod)
                                    Chunks[i].Lod = 2;
                                else if (cameraDist > 576 * 576 && GameSettings.Lod)
                                    Chunks[i].Lod = 4;
                                else
                                    Chunks[i].Lod = _player.IsGliding ? 2 : 1;
                            }


                            if (Chunks[i] != null && Chunks[i].Initialized && Chunks[i].IsGenerated &&
                                Chunks[i].Landscape.StructuresPlaced && !World.MeshQueue.Contains(Chunks[i]) &&
                                (!Chunks[i].BuildedCompletely || Chunks[i].Lod != Chunks[i].BuildedLod || Chunks[i].NeedsRebuilding) 
                                || Chunks[i] != null && Chunks[i].Initialized && Chunks[i].Mesh.Crashed)
                            {
                                if (Chunks[i] != null && Chunks[i].NeighboursExist)
                                {
                                    World.AddChunkToQueue(Chunks[i], true);
                                }
                            }
                        }
                        _left = 0f;
                    }
                    if (ActiveChunks == _prevChunkCount) continue;
                    _prevChunkCount = ActiveChunks;
                    this.UpdateFog();
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                new Thread(ManageChunksMesh).Start();
            }
        }

        private void ManageChunks()
        {
            try
            {
                while (Program.GameWindow.Exists)
                {
                    Thread.Sleep(250);
                    _genCounter += 250; 

                    if (!World.IsGenerated || !Enabled)
                        continue;

                    Offset = World.ToChunkSpace(_player.BlockPosition);
                    Chunk UnderChunk = World.GetChunkByOffset(Offset);
                    var newPos = _player.BlockPosition.Xz;

                    if ( (newPos - _lastPos2).LengthSquared > 16*16 || GameSettings.ChunkLoaderRadius != _lastRadius ||
                        (UnderChunk != null && !UnderChunk.IsGenerated) || World.Seed != _previousSeed || _genCounter >= 1000)
                    {
                        _genCounter = 0;
                        _previousSeed = World.Seed;
                        Chunk[] Chunks;
                        lock (World.Chunks)
                        {
                            Chunks = World.Chunks.ToArray();
                        }
                        for (int i = Chunks.Length - 1; i > -1; i--)
                        {
                            if (Chunks[i].Disposed)
                            {
                                World.RemoveChunk(Chunks[i]);
                                continue;
                            }

                            if ((Chunks[i].Position.Xz - _player.Position.Xz).LengthSquared >
                                GameSettings.ChunkLoaderRadius * .5f * Chunk.Width *
                                GameSettings.ChunkLoaderRadius * .5f * Chunk.Width)
                            {
                                if (!Chunks[i].Blocked) World.RemoveChunk(Chunks[i]);
                                continue;
                            }
                            if (!Chunks[i].Initialized) Chunks[i].Initialize();

                            if (!Chunks[i].IsGenerated || !Chunks[i].Mesh.Enabled || !Chunks[i].Landscape.StructuresPlaced)
                            {
                                if ((!Chunks[i].IsGenerated || !Chunks[i].Landscape.StructuresPlaced) &&
                                    !World.ChunkGenerationQueue.Queue.Contains(Chunks[i]))
                                {
                                    World.AddChunkToQueue(Chunks[i], false);
                                }
                            }
                        }
                        _lastPos2 = newPos;
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
            }
        }
    }
}