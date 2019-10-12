using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.PlantSystem
{
    public abstract class PlantDesign
    {
        public abstract CacheItem Type { get; }

        public VertexData Model => CacheManager.GetModel(Type);

        public abstract Matrix4 TransMatrix(Vector3 Position, Random Rng);

        public abstract NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng);

        public virtual void AddShapes(Chunk UnderChunk, Matrix4 TransMatrix){}

        public virtual bool HasCustomPlacement => false;
        
        public virtual bool AffectedByLod => true;
        
        public virtual void CustomPlacement(NativeVertexData Data, Matrix4 TransMatrix, Chunk UnderChunk)
            => throw new NotImplementedException();
    }
}
