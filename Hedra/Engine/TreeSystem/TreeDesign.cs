using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Rendering;

namespace Hedra.Engine.TreeSystem
{
    public abstract class TreeDesign
    {
        public abstract float Spacing { get; }

        public abstract VertexData Model { get; }

        public abstract VertexData Paint(VertexData Data, Vector4 WoodColor, Vector4 LeafColor);

        public virtual List<CollisionShape> GetShapes(VertexData Data)
        {
            return CacheManager.GetShape(Data);
        }
    }
}