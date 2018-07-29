using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageCache
    {
        public static MarketCache Market { get; }
        private readonly Dictionary<string, List<CollisionShape>> _colliderCache;
        private readonly Dictionary<string, VertexData> _modelCache;
        private readonly Dictionary<string, Vector3> _sizeCache;

        static VillageCache()
        {
            Market = new MarketCache();
        }

        private VillageCache()
        {
            _colliderCache = new Dictionary<string, List<CollisionShape>>();
            _modelCache = new Dictionary<string, VertexData>();
            _sizeCache = new Dictionary<string, Vector3>();
        }

        public List<CollisionShape> GrabShapes(string Path)
        {
            return _colliderCache[Path].DeepClone();
        }
        
        public VertexData GrabModel(string Path)
        {
            return _modelCache[Path].ShallowClone();
        }
        
        public Vector3 GrabSize(string Path)
        {
            return _sizeCache[Path];
        }
        
        public static VillageCache FromTemplate(VillageTemplate Template)
        {
            var cache = new VillageCache();
            var designs = Template.Designs;
            for (var i = 0; i < designs.Length; i++)
            {
                for (var j = 0; j < designs[i].Length; j++)
                {
                    cache._colliderCache.Add(designs[i][j].Path, AssetManager.LoadCollisionShapes(designs[i][j].Path, Vector3.One * designs[i][j].Scale));
                    cache._modelCache.Add(designs[i][j].Path, AssetManager.PLYLoader(designs[i][j].Path, Vector3.One * designs[i][j].Scale));
                    cache._sizeCache.Add(designs[i][j].Path, CalculateBounds(cache._modelCache[designs[i][j].Path]));
                }
            }
            return cache;
        }

        private static Vector3 CalculateBounds(VertexData Model)
        {
            return new Vector3(
                Model.SupportPoint(Vector3.UnitX).X - Model.SupportPoint(-Vector3.UnitX).X,
                Model.SupportPoint(Vector3.UnitY).Y - Model.SupportPoint(-Vector3.UnitY).Y,
                Model.SupportPoint(Vector3.UnitZ).Z - Model.SupportPoint(-Vector3.UnitZ).Z
            );
        }
    }
}