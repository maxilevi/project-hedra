using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Hedra.Engine.IO;
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

        public VertexData GetOrCreate(string Path, Vector3 Scale)
        {
            if (_modelCache.ContainsKey(Path)) return GrabModel(Path);
            _modelCache.Add(Path, AssetManager.PLYLoader(Path, Vector3.One * Scale).AsCompressed());
            return GetOrCreate(Path, Scale);
        }
        
        public List<CollisionShape> GetOrCreateShapes(string Path, Vector3 Scale)
        {
            if (_colliderCache.ContainsKey(Path)) return GrabShapes(Path);
            _colliderCache.Add(Path, AssetManager.LoadCollisionShapes(Path, Vector3.One * Scale));
            return GetOrCreateShapes(Path, Scale);
        }
        
        public static VillageCache FromTemplate(VillageTemplate Template)
        {
            var sw = new Stopwatch();
            sw.Start();
            var cache = new VillageCache();
            var designs = Template.CacheableDesigns;
            for (var i = 0; i < designs.Length; i++)
            {
                for (var j = 0; j < designs[i].Length; j++)
                {
                    var offset = designs[i][j].Offset * designs[i][j].Scale;
                    var offsetMatrix = Matrix4.CreateTranslation(offset);
                    cache._colliderCache.Add(
                        designs[i][j].Path,
                        AssetManager.LoadCollisionShapes(designs[i][j].Path, Vector3.One * designs[i][j].Scale).Select(S => S.Transform(offsetMatrix)).ToList()
                    );
                    cache._modelCache.Add(
                        designs[i][j].Path,
                        AssetManager.PLYLoader(designs[i][j].Path, Vector3.One * designs[i][j].Scale, offset, Vector3.Zero).AsCompressed()
                    );
                    cache._sizeCache.Add(designs[i][j].Path, CalculateBounds(cache._modelCache[designs[i][j].Path].ToVertexData()));
                    if(designs[i][j].LodPath != null)
                        cache._modelCache.Add(designs[i][j].LodPath, AssetManager.PLYLoader(designs[i][j].LodPath, Vector3.One * designs[i][j].Scale).AsCompressed());
                }
            }
            sw.Stop();
            Log.WriteLine($"Loading village '{Template.Name}' took '{sw.ElapsedMilliseconds}' MS");
            return cache;
        }

        private static VertexData LoadCollisionShapesAsVertexData(string Filename, Vector3 Scale, Vector3 Position, Vector3 Rotation)
        {
            var model = new VertexData();
            string name = Path.GetFileNameWithoutExtension(Filename);
            var iterator = 0;
            while(true)
            {
                var path = $"Assets/Env/Colliders/{name}_Collider{iterator}.ply";
                var data = AssetManager.ReadBinary(path, AssetManager.AssetsResource);
                if (data == null) break;
                var vertexInformation = AssetManager.PLYLoader(path, Scale, Position, Rotation);
                model += vertexInformation;
                iterator++;
            }
            model.Colors = Enumerable.Repeat(Vector4.One, model.Vertices.Count).ToList();
            return model;
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