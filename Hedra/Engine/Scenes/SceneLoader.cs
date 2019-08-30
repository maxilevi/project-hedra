using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Scenes
{
    public static class SceneLoader
    {
        private static readonly Vector3 LightColorCode = new Vector3(1, 0, 1);
        private static readonly Vector3 PunchingBag = new Vector3(0, 1, 0);

        public static void Load(CollidableStructure Structure, VertexData Scene)
        {
            Load(Structure, Scene, new SceneSettings());
        }

        public static void LoadIfExists(CollidableStructure Structure, string Path, Vector3 Scale, Matrix4 Transformation)
        {
            LoadIfExists(Structure, Path, Scale, Transformation, new SceneSettings());
        }
        
        public static void LoadIfExists(CollidableStructure Structure, string Filename, Vector3 Scale, Matrix4 Transformation, SceneSettings Settings)
        {
            var path = $"{Path.GetDirectoryName(Filename)}/{Path.GetFileNameWithoutExtension(Filename)}-Scene.ply";
            var scene = AssetManager.ModelExists(path) ? AssetManager.PLYLoader(path, Scale) : null;
            if (scene != null)
            {
                scene.Transform(Transformation);
                Load(Structure, scene, Settings);
            }
        }
        
        public static void Load(CollidableStructure Structure, VertexData Scene, SceneSettings Settings)
        {
            var parts = Scene.Ungroup();
            var lights = LoadLights(
                parts.Where(V => V.IsColorCode(LightColorCode)).Select(V => V.AverageVertices()).ToArray(),
                Settings
            );
            var punchingBags = LoadPunchingBags(
                parts.Where(V => V.IsColorCode(PunchingBag)).Select(V => V.Vertices.ToArray()).ToArray()
            );
            Structure.WorldObject.AddChildren(punchingBags);
        }

        private static BaseStructure[] LoadPunchingBags(Vector3[][] VertexGroups)
        {
            var list = new List<BaseStructure>();
            for (var i = 0; i < VertexGroups.Length; ++i)
            {
                var box = Physics.BuildDimensionsBox(new VertexData
                {
                    Vertices = VertexGroups[i].ToList()
                }) * 2;
                list.Add(new PunchingBag(VertexGroups[i].AverageVertices(), box));
            }
            return list.ToArray();
        }
        
        private static BaseStructure[] LoadLights(Vector3[] Points, SceneSettings Settings)
        {
            var list = new List<BaseStructure>();
            for (var i = 0; i < Points.Length; ++i)
            {
                list.Add(new WorldLight(Points[i])
                {
                    DisableAtNight = Settings.DisableLightsAtNight,
                    LightColor = Settings.LightColor,
                    Radius = Settings.LightRadius
                });
            }
            return list.ToArray();
        }
    }
}