using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public interface ICacheProvider
    {
        void Load();
        
        VertexData GetModel(string Type);

        VertexData GetPart(string Type, VertexData Model);

        List<CollisionShape> GetShape(VertexData Model);

        List<CollisionShape> GetShape(string Type, VertexData Data);

        void Discard();

        void Check(InstanceData Data);
        
        Dictionary<object, List<float>> CachedExtradata { get; }
        
        Dictionary<object, List<Vector4>> CachedColors { get; }
        
        int UsedBytes { get; }
    }
}