using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public abstract class PlantDesign
    {
        public abstract CacheItem Type { get; }

        public VertexData Model => CacheManager.GetModel(Type);

        public virtual bool HasCustomPlacement => false;

        public virtual bool AffectedByLod => true;

        public abstract Matrix4x4 TransMatrix(Vector3 Position, Random Rng);

        public abstract NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng);

        public virtual void AddShapes(Chunk UnderChunk, Matrix4x4 TransMatrix)
        {
        }

        public virtual void CustomPlacement(NativeVertexData Data, Matrix4x4 TransMatrix, Chunk UnderChunk)
        {
            throw new NotImplementedException();
        }
    }
}