using System.Collections.Generic;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public static class CacheManager
    {
        public static ICacheProvider Provider { get; set; } = new CacheProvider();

        public static Dictionary<string, List<CompressedValue<float>>> CachedExtradata => Provider.CachedExtradata;
        
        public static Dictionary<string, List<CompressedValue<Vector4>>> CachedColors => Provider.CachedColors;
        
        public static void Load()
        {
            Provider.Load();
        }

        public static VertexData GetModel(CacheItem Item)
        {
            return Provider.GetModel(Item.ToString().ToLowerInvariant());
        }
        
        public static VertexData GetPart(CacheItem Item, VertexData Model)
        {
            return Provider.GetPart(Item.ToString().ToLowerInvariant(), Model);
        }

        public static List<CollisionShape> GetShape(VertexData Model)
        {
            return Provider.GetShape(Model);
        }

        public static List<CollisionShape> GetShape(CacheItem Item, VertexData Model)
        {
            return GetShape(Item.ToString(), Model);
        }

        public static List<CollisionShape> GetShape(string Type, VertexData Data)
        {
            return Provider.GetShape(Type.ToLowerInvariant(), Data);
        }

        public static void Discard()
        {
            Provider.Discard();
        }

        public static void Check(InstanceData Data)
        {
            Provider.Check(Data);
        }
    }
}