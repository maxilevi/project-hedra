using System.IO;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    public class WitchHutCache : CacheType
    {
        private static readonly VertexData _hut0Door0;
        private static readonly VertexData _hut0Door1;

        static WitchHutCache()
        {
            _hut0Door0 = AssetManager.PLYLoader("Assets/Env/Structures/WitchHut/WitchHut0_Door0.ply", Scale);
            _hut0Door0.Translate(Offset);
            _hut0Door1 = AssetManager.PLYLoader("Assets/Env/Structures/WitchHut/WitchHut0_Door1.ply", Scale);
            _hut0Door1.Translate(Offset);
        }

        public WitchHutCache()
        {
            var model = AssetManager.PLYLoader("Assets/Env/Structures/WitchHut/WitchHut0.ply", Scale);
            model.Translate(Offset);
            AddModel(model);

            var shapes = AssetManager.LoadCollisionShapes("Assets/Env/Structures/WitchHut/WitchHut0.ply", Scale);
            shapes.ForEach(S => S.Transform(Offset * Scale));
            AddShapes(shapes);
        }

        public override CacheItem Type => CacheItem.WitchHut;

        public static Vector3 Hut0StealPosition => new Vector3(4.46797f, 0.2f, 5.31457f) * Scale;
        public static Vector3 Hut0Witch0Position => new Vector3(0, 0, -5.5f) * Scale;
        public static Vector3 Hut0Witch1Position => new Vector3(8, 0, -15f) * Scale;

        public static Vector3 PlantOffset { get; } = new Vector3(0, 0, .5f) * Scale;

        public static Vector3[] PlantRows { get; } =
        {
            new Vector3(-3.5f, 0.1f, -5f) * Scale + Offset,
            new Vector3(-5.25f, 0.1f, -5f) * Scale + Offset,
            new Vector3(-6.5f, 0.1f, -5f) * Scale + Offset,
            new Vector3(-7.75f, 0.1f, -5f) * Scale + Offset,
            new Vector3(-9.25f, 0.1f, -5f) * Scale + Offset,
            new Vector3(-10.5f, 0.1f, -5f) * Scale + Offset
        };

        public static int[] PlantWidths { get; } =
        {
            4,
            4,
            5,
            5,
            5,
            5
        };

        public static Vector3 Offset => Vector3.UnitY * .2f;
        public static Vector3 Scale => Vector3.One * 3.5f;

        public static VertexData Hut0Door0 => _hut0Door0.Clone();
        public static VertexData Hut0Door1 => _hut0Door1.Clone();
        public static Vector3 Hut0Door0Position => new Vector3(0.10415f, 1.57616f, -0.01544f) * Scale + Offset;
        public static Vector3 Hut0Door1Position => new Vector3(-0.70139f, 0.98168f, -10.69296f) * Scale + Offset;
        public static bool Hut0Door0InvertedPivot => true;
        public static bool Hut0Door1InvertedPivot => true;
        public static bool Hut0Door0InvertedRotation => false;
        public static bool Hut0Door1InvertedRotation => false;

        private static VertexData LoadCollisionShapesAsVertexData(string Filename, Vector3 Scale)
        {
            var model = new VertexData();
            var name = Path.GetFileNameWithoutExtension(Filename);
            var iterator = 0;
            while (true)
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
    }
}