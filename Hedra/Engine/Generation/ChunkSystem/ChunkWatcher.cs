using System;
using Hedra.Engine.Management;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public delegate void OnChunkEvent(Chunk Object);

    public class ChunkWatcher : IDisposable
    {
        private Chunk _object;
        private bool _wasBuilt;
        private bool _wasKilled;

        public ChunkWatcher(Chunk Object)
        {
            _object = Object;
        }

        public bool Disposed { get; private set; }
        public bool IsHealthy { get; private set; }

        public void Dispose()
        {
            _wasKilled = true;
            Disposed = true;
        }

        public event OnChunkEvent OnChunkReady;

        public void Update()
        {
            if (_wasKilled) _object = null;
            if (GameManager.Player == null) return;
            IsHealthy = (_object?.BuildedWithStructures ?? false) && !Disposed;
            if (_object?.Disposed ?? true) Dispose();
            if (_object == null || Disposed) return;

            ManageLod();
            var result = ManageState();
            if (!result) return;
            if (WasChunkGenerated(_object) && ShouldWeRebuildChunk(_object))
                World.AddChunkToQueue(_object, ChunkQueueType.Mesh);
            if (!_wasBuilt && _object.BuildedWithStructures)
            {
                _wasBuilt = true;
                OnChunkReady?.Invoke(_object);
            }
        }

        private bool ManageState()
        {
            if (_object == null) return false;
            if (_object.Disposed)
            {
                Kill();
                return false;
            }

            var offset = World.ToChunkSpace(GameManager.Player.Position);
            var radius = GameSettings.ChunkLoaderRadius * .5f * Chunk.Width;
            if ((_object.Position.Xz() - offset).LengthSquared() > radius * radius)
            {
                Kill();
                return false;
            }

            if (!_object.IsGenerated || !_object.Landscape.StructuresPlaced)
            {
                World.AddChunkToQueue(_object, ChunkQueueType.Generation);
                return false;
            }

            return true;
        }

        private void ManageLod()
        {
            var cameraDist = (_object.Position.Xz() - World.ToChunkSpace(GameManager.Player.Position)).LengthSquared();
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
            _object.Lod = newLod;
        }

        private static bool WasChunkGenerated(Chunk Chunk)
        {
            return Chunk != null && Chunk.IsGenerated && Chunk.Landscape.StructuresPlaced;
        }

        private static bool ShouldWeRebuildChunk(Chunk Chunk)
        {
            return (!Chunk.BuildedCompletely || Chunk.Lod != Chunk.BuildedLod || Chunk.NeedsRebuilding) &&
                   Chunk.NeighboursExist;
        }

        public void Kill()
        {
            var toDelete = _object;
            Executer.ExecuteOnMainThread(() =>
            {
                if (!toDelete.Disposed)
                    World.RemoveChunk(toDelete);
                Dispose();
            });
        }
    }
}