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
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public abstract class PlantDesign
    {
        public abstract VertexData Model { get; }

        public abstract Matrix4 TransMatrix(Vector3 Position, Random Rng);

        public abstract VertexData Paint(Vector3 Position, VertexData Data, Region Region, Random Rng);

        public virtual void AddShapes(Chunk UnderChunk, Matrix4 TransMatrix){}

        public virtual bool HasCustomPlacement { get; } = false;
        public virtual void CustomPlacement(VertexData Data, Matrix4 TransMatrix) {}
    }
}
