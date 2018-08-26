﻿using System;
using System.Dynamic;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public delegate void OnChunkEvent(Chunk Object);
        
    public class ChunkWatcher : IDisposable
    {
        private Chunk _object;
        private bool _wasBuilded;
        public event OnChunkEvent OnChunkReady;
        public bool Disposed { get; private set; }
        public bool IsHealthy { get; private set; }

        public ChunkWatcher(Chunk Object)
        {
            _object = Object;
        }

        public void Update()
        {
            if(GameManager.Player == null) return;
            IsHealthy = (_object?.BuildedWithStructures ?? false) && !Disposed;
            if (_object?.Disposed ?? true) this.Dispose();
            if (_object == null || Disposed) return;

            var result = this.ManageState();
            if(!result) return;
            this.ManageLod();

            if (WasChunkBuilt(_object) && ShouldWeRebuildChunk(_object))
            {
                World.AddChunkToQueue(_object, true);
            }
            if (_object.BuildedWithStructures && !_wasBuilded)
            {
                _wasBuilded = true;
                OnChunkReady?.Invoke(_object);
            }
        }

        private bool ManageState()
        {
            if(_object.Disposed)
            {
                this.Kill();
                return false;
            }

            var offset = World.ToChunkSpace(GameManager.Player.Position);
            var radius = GameSettings.ChunkLoaderRadius * .5f * Chunk.Width;
            if ((_object.Position.Xz - offset).LengthSquared > radius * radius)
            {
                this.Kill();
                return false;
            }
            if (!_object.Initialized) _object.Initialize();
            if (!_object.IsGenerated || !_object.Landscape.StructuresPlaced)
            {
                World.AddChunkToQueue(_object, false);
                return false;
            }
            return true;
        }

        private void ManageLod()
        {
            if (!_object.IsGenerated) return;
            var cameraDist = (_object.Position.Xz - GameManager.Player.View.CameraPosition.Xz).LengthSquared;
            if (cameraDist > 288 * 288 && cameraDist < 576 * 576 && GameSettings.Lod)
                _object.Lod = 2;
            else if (cameraDist > 576 * 576 && GameSettings.Lod)
                _object.Lod = 4;
            else
                _object.Lod = GameManager.Player.IsGliding ? 2 : 1;
        }

        private static bool WasChunkBuilt(Chunk Chunk)
        {
            return Chunk != null && Chunk.Initialized && Chunk.IsGenerated && Chunk.Landscape.StructuresPlaced;
        }

        private static bool ShouldWeRebuildChunk(Chunk Chunk)
        {
            return (!Chunk.BuildedCompletely || Chunk.Lod != Chunk.BuildedLod || Chunk.NeedsRebuilding) && Chunk.NeighboursExist;
        }

        public void Kill()
        {
            World.RemoveChunk(_object);
            this.Dispose();
        }

        public void Dispose()
        {
            _object = null;
            Disposed = true;
        }
    }
}
