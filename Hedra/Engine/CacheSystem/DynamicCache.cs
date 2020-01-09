using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public static class DynamicCache
    {
        private static readonly Dictionary<string, CachedVertexData> _modelCache = new Dictionary<string, CachedVertexData>();
        private static readonly Dictionary<string, List<CollisionShape>> _shapeCache = new Dictionary<string, List<CollisionShape>>();
        private static readonly object _modelLock = new object();
        private static readonly object _shapesLock = new object();
        public static VertexData Get(string Path, Vector3 Scale)
        {
            lock (_modelLock)
            {
                if (!_modelCache.ContainsKey(Path))
                {
                    var cachedModel = CachedVertexData.FromVertexData(AssetManager.LoadPLYWithLODs(Path, Vector3.One));
                    _modelCache.Add(Path, cachedModel);
                    UsedBytes += cachedModel.SizeInBytes;
                }

                var model = _modelCache[Path].ToVertexData().ShallowClone();
                model.Scale(Scale);
                return model;
            }
        }

        public static List<CollisionShape> GetShapes(string Path, Vector3 Scale)
        {
            lock (_shapesLock)
            {
                if (Path.Contains("Colliders.ply"))
                    throw new ArgumentException("Provided path should be the mesh path.");
                if (!_shapeCache.ContainsKey(Path))
                {
                    var cachedShapes = AssetManager.LoadCollisionShapes(Path, Vector3.One);
                    _shapeCache.Add(Path, cachedShapes);
                    UsedBytes += cachedShapes.Sum(S => S.SizeInBytes);
                }

                var shapes = _shapeCache[Path].DeepClone();
                return shapes.Select(S => S.Transform(Matrix4x4.CreateScale(Scale))).ToList();
            }
        }
        
        public static int UsedBytes { get; private set; }
    }
}