using System.IO;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class WitchHutCache : CacheType
    {
        public override CacheItem Type => CacheItem.WitchHut;
        private static readonly VertexData _hut0Door0;
        private static readonly VertexData _hut0Door1;
        static WitchHutCache()
        {
            _hut0Door0 = AssetManager.PLYLoader("Assets/Env/Structures/WitchHut0_Door0.ply", Scale);
            _hut0Door0.Translate(Offset);
            _hut0Door1 = AssetManager.PLYLoader("Assets/Env/Structures/WitchHut0_Door1.ply", Scale);
            _hut0Door1.Translate(Offset);
        }

        public WitchHutCache()
        {
            var model = AssetManager.PLYLoader("Assets/Env/Structures/WitchHut0.ply", Scale);
            model.Translate(Offset);
            AddModel(model);

            var shapes = AssetManager.LoadCollisionShapes("Assets/Env/Structures/WitchHut0.ply", Scale);
            shapes.ForEach(S => S.Transform(Offset * Scale));
            AddShapes(shapes);
        }
        
        private static Vector3 Offset => Vector3.UnitY * .15f;
        private static Vector3 Scale => Vector3.One * 3.25f;
        
        private static VertexData LoadCollisionShapesAsVertexData(string Filename, Vector3 Scale)
        {
            var model = new VertexData();
            string name = Path.GetFileNameWithoutExtension(Filename);
            var iterator = 0;
            while(true)
            {
                var path = $"Assets/Env/Colliders/{name}_Collider{iterator}.ply";
                var data = AssetManager.ReadBinary(path, AssetManager.AssetsResource);
                if (data == null) break;
                var vertexInformation = AssetManager.PLYLoader(path, Scale);
                model += vertexInformation;
                iterator++;
            }
            model.Colors = Enumerable.Repeat(Vector4.One, model.Vertices.Count).ToList();
            return model;
        }
        
        public static VertexData Hut0Door0 => _hut0Door0.Clone();
        public static VertexData Hut0Door1 => _hut0Door1.Clone();
        public static Vector3 Hut0Door0Position => new Vector3(0.10415f, 1.57616f, -0.01544f) * Scale;
        public static Vector3 Hut0Door1Position => new Vector3(-0.70139f, 0.98168f, -10.69296f) * Scale;
        public static bool Hut0Door0InvertedPivot => true;
        public static bool Hut0Door1InvertedPivot => true;
        public static bool Hut0Door0InvertedRotation => false;
        public static bool Hut0Door1InvertedRotation => false;
    }
}