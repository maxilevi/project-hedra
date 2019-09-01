using System;
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
        private static readonly Vector3 TrainingDummyColorCode = new Vector3(0, 1, 0);
        private static readonly Vector3 WellColorCode = new Vector3(0, 0, 1);
        private static readonly Vector3 NPCPositionColorCode = new Vector3(0, 0, 0);

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
            var map = new Dictionary<Vector3, List<VertexData>>
            {
                {LightColorCode, new List<VertexData>()},
                {TrainingDummyColorCode, new List<VertexData>()},
                {WellColorCode, new List<VertexData>()},
                {NPCPositionColorCode, new List<VertexData>()}
            };
            for (var i = 0; i < parts.Length; ++i)
            {
                if(!parts[i].HasColors) throw new ArgumentOutOfRangeException("Scene mesh doesn't have colors");
                var averageColor = parts[i].Colors.Select(V => V.Xyz).Aggregate((V1, V2) => V1 + V2) / parts[i].Colors.Count;
                map[averageColor].Add(parts[i]);
            }
            var lights = LoadLights(
                map[LightColorCode].Select(V => V.AverageVertices()).ToArray(),
                Settings
            );
            Structure.WorldObject.AddChildren(lights);
            var punchingBags = LoadPunchingBags(
                map[TrainingDummyColorCode].Select(V => V.Vertices.ToArray()).ToArray()
            );
            Structure.WorldObject.AddChildren(punchingBags);
            var wells = LoadWells(
                map[WellColorCode].Select(V => V.Vertices.ToArray()).ToArray()
            );
            Structure.WorldObject.AddChildren(punchingBags);
        }

        private static BaseStructure[] LoadWells(Vector3[][] VertexGroups)
        {
            var list = new List<BaseStructure>();
            for (var i = 0; i < VertexGroups.Length; ++i)
            {
                var averageCenter = VertexGroups[i].AverageVertices();
                var radius = new CollisionShape(VertexGroups[i]).BroadphaseRadius * 2;
                list.Add(new Well(averageCenter, radius));
            }
            return list.ToArray();
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
                    IsNightLight = Settings.IsNightLight,
                    LightColor = Settings.LightColor,
                    Radius = Settings.LightRadius
                });
            }
            return list.ToArray();
        }
    }
}