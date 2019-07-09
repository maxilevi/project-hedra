using System;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Game;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public delegate void OnChunkEvent(Chunk Object);
        
    public class ChunkWatcher : IDisposable
    {
        public static event OnChunkEvent OnChunkLodChanged;
        private Chunk _object;
        private bool _wasBuilt;
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

            ManageLod();
            var result = ManageState();
            if(!result) return;
            if (WasChunkGenerated(_object) && ShouldWeRebuildChunk(_object))
            {
                World.AddChunkToQueue(_object, true);
            }
            if (!_wasBuilt && _object.BuildedWithStructures)
            {
                _wasBuilt = true;
                OnChunkReady?.Invoke(_object);
            }
        }

        private bool ManageState()
        {
            if (_object == null) return false;
            if(_object.Disposed)
            {
                Kill();
                return false;
            }

            var offset = World.ToChunkSpace(GameManager.Player.Position);
            var radius = GameSettings.ChunkLoaderRadius * .5f * Chunk.Width;
            if ((_object.Position.Xz - offset).LengthSquared > radius * radius)
            {
                Kill();
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
            var cameraDist = (_object.Position.Xz - World.ToChunkSpace(GameManager.Player.Position)).LengthSquared;
            var newLod = -1;
            if (cameraDist <= GeneralSettings.Lod1DistanceSquared)
                newLod = 1;
            else if (cameraDist <= GeneralSettings.Lod2DistanceSquared)
                newLod = 2;
            else if (cameraDist <= GeneralSettings.Lod3DistanceSquared)
                newLod = 4;
            else if (cameraDist > GeneralSettings.Lod3DistanceSquared)
                newLod = 8;
            else
                throw new ArgumentOutOfRangeException("Unsupported LOD.");
            if(_object.Lod != newLod) OnChunkLodChanged?.Invoke(_object);
            _object.Lod = newLod;
        }

        private static bool WasChunkGenerated(Chunk Chunk)
        {
            return Chunk != null && Chunk.Initialized && Chunk.IsGenerated && Chunk.Landscape.StructuresPlaced;
        }

        private static bool ShouldWeRebuildChunk(Chunk Chunk)
        {
            return (!Chunk.BuildedCompletely || Chunk.Lod != Chunk.BuildedLod || Chunk.NeedsRebuilding) && Chunk.NeighboursExist;
        }

        public void Kill()
        {
            var toDelete = _object;
            Executer.ExecuteOnMainThread(() => World.RemoveChunk(toDelete));
            Dispose();
        }

        public void Dispose()
        {
            _object = null;
            Disposed = true;
        }
    }
}
