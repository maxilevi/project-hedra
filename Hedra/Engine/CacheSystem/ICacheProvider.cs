using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public interface ICacheProvider
    {
        void Load();
        
        VertexData GetModel(string Type);

        List<CollisionShape> GetShape(VertexData Model);

        List<CollisionShape> GetShape(string Type, VertexData Data);

        void Discard();

        void Check(InstanceData Data);
        
        Dictionary<float, List<CompressedValue<float>>> CachedExtradata { get; }
        
        Dictionary<Vector4, List<CompressedValue<Vector4>>> CachedColors { get; }
    }
}