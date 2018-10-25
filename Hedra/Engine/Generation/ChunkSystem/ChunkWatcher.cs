using System;
using Hedra.Engine.Game;

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

            this.ManageLod();
            var result = this.ManageState();
            if(!result) return;

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
            if (!_object.IsGenerated || !_object.Landscape.StructuresPlaced || _object.Landscape.HasToGenerateMoreData)
            {
                World.AddChunkToQueue(_object, false);
                return false;
            }
            return true;
        }

        private void ManageLod()
        {
            if (!GameSettings.Lod) return;
            var cameraDist = (_object.Position.Xz - GameManager.Player.View.CameraPosition.Xz).LengthSquared;
            if (cameraDist > 288 * 288 && cameraDist < 512 * 512)
                _object.Lod = 2;
            else if (cameraDist > 512 * 512 && cameraDist < 1024 * 1024)
                _object.Lod = 4;
            else if (cameraDist > 1024 * 1024 )
                _object.Lod = 8;
            else
                _object.Lod = 1;
            //_object.HasLodedElements
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
