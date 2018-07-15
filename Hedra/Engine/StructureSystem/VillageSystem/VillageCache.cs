using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageCache
    {

        private readonly Dictionary<string, List<CollisionShape>> _colliderCache;
        private readonly Dictionary<string, VertexData> _modelCache;
        
        private VillageCache()
        {
            _colliderCache = new Dictionary<string, List<CollisionShape>>();
        }

        public List<CollisionShape> GrabShapes(string Path)
        {
            return _colliderCache[Path].DeepClone();
        }
        
        public VertexData GrabModel(string Path)
        {
            return _modelCache[Path].ShallowClone();
        }
        
        public static VillageCache FromTemplate(VillageTemplate Template)
        {
            var cache = new VillageCache();
            var buildings = Template.Buildings;
            for (var i = 0; i < buildings.Length; i++)
            {
                for (var j = 0; j < buildings[i].Designs.Length; j++)
                {
                    var design = buildings[i].Designs[j];
                    cache._colliderCache.Add(design.Path, AssetManager.LoadCollisionShapes(design.Path, Vector3.One * design.Scale));
                    cache._modelCache.Add(design.Path, AssetManager.PlyLoader(design.Path, Vector3.One * design.Scale));   
                }
            }
            return cache;
        }
    }
}