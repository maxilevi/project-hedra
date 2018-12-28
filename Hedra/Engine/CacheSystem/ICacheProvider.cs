using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

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
        
        Dictionary<object, List<CompressedValue<float>>> CachedExtradata { get; }
        
        Dictionary<object, List<CompressedValue<Vector4>>> CachedColors { get; }
    }
}