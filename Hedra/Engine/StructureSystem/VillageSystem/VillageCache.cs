using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class VillageCache
    {
        public static MarketCache Market { get; }
        private readonly Dictionary<string, List<CollisionShape>> _colliderCache;
        private readonly Dictionary<string, CompressedVertexData> _modelCache;
        private readonly Dictionary<string, Vector3> _sizeCache;

        static VillageCache()
        {
            Market = new MarketCache();
        }

        private VillageCache()
        {
            _colliderCache = new Dictionary<string, List<CollisionShape>>();
            _modelCache = new Dictionary<string, CompressedVertexData>();
            _sizeCache = new Dictionary<string, Vector3>();
        }

        public List<CollisionShape> GrabShapes(string Path)
        {
            return _colliderCache[Path].DeepClone();
        }
        
        public VertexData GrabModel(string Path)
        {
            return _modelCache[Path].ToVertexData().ShallowClone();
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
                    cache._modelCache.Add(designs[i][j].Path, AssetManager.PLYLoader(designs[i][j].Path, Vector3.One * designs[i][j].Scale).AsCompressed());
                    cache._sizeCache.Add(designs[i][j].Path, CalculateBounds(cache._modelCache[designs[i][j].Path].ToVertexData()));
                }
            }
            return cache;
        }

        private static VertexData LoadCollisionShapesAsVertexData(string Filename, Vector3 Scale)
        {
            var model = new VertexData();
            string name = Path.GetFileNameWithoutExtension(Filename);
            var iterator = 0;
            while(true)
            {
                var path = $"Assets/Env/Colliders/{name}_Collider{iterator}.ply";
                var data = AssetManager.ReadBinary(path, AssetManager.AssetsResource);
                if(data == null) return model;
                var vertexInformation = AssetManager.PLYLoader(path, Scale);
                vertexInformation.Colors = Enumerable.Repeat(Vector4.One, vertexInformation.Vertices.Count).ToList();
                model += vertexInformation;
                iterator++;
            }
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