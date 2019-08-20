using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public static class DynamicCache
    {
        private static readonly Dictionary<string, CachedVertexData> _modelCache = new Dictionary<string, CachedVertexData>();
        private static readonly Dictionary<string, List<CollisionShape>> _shapeCache = new Dictionary<string, List<CollisionShape>>();
        public static VertexData Get(string Path, Vector3 Scale)
        {
            if (!_modelCache.ContainsKey(Path))
            {
                _modelCache.Add(Path, CachedVertexData.FromVertexData(AssetManager.LoadPLYWithLODs(Path, Vector3.One)));
            }
            var model = _modelCache[Path].ToVertexData().ShallowClone();
            model.Scale(Scale);
            return model;
        }

        public static List<CollisionShape> GetShapes(string Path, Vector3 Scale)
        {
            if (!_shapeCache.ContainsKey(Path))
            {
                _shapeCache.Add(Path, AssetManager.LoadCollisionShapes(Path, Vector3.One));
            }

            var shapes = _shapeCache[Path].DeepClone();
            return shapes.Select(S => S.Transform(Matrix4.CreateScale(Scale))).ToList();
        }
    }
}