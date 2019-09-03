using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hedra.Components;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Mission;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Scenes
{
    public static class SceneLoader
    {
        private static readonly Vector3 LightColorCode = new Vector3(1, 0, 1);
        private static readonly Vector3 TrainingDummyColorCode = new Vector3(0, 1, 0);
        private static readonly Vector3 WellColorCode = new Vector3(0, 0, 1);
        private static readonly Vector3 BoatMerchantColorCode = new Vector3(0, 0, 0);
        private static readonly Vector3 FishermanColorCode = new Vector3(1, 1, 0);

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
                {BoatMerchantColorCode, new List<VertexData>()},
                {FishermanColorCode, new List<VertexData>()}
            };
            for (var i = 0; i < parts.Length; ++i)
            {
                if(!parts[i].HasColors) throw new ArgumentOutOfRangeException("Scene mesh doesn't have colors");
                var averageColor = parts[i].Colors.Select(V => V.Xyz).Aggregate((V1, V2) => V1 + V2) / parts[i].Colors.Count;
                map[averageColor].Add(parts[i]);
            }
            /* Add Lights */
            var lights = LoadLights(
                map[LightColorCode].Select(V => V.AverageVertices()).ToArray(),
                Settings
            );
            Structure.WorldObject.AddChildren(lights);

            /* Add Wells */
            var wells = LoadWells(
                map[WellColorCode].Select(V => V.Vertices.ToArray()).ToArray()
            );
            Structure.WorldObject.AddChildren(wells);

            /* Add punching bags */
            var bags = LoadPunchingBags(
                map[TrainingDummyColorCode].Select(V => V.Vertices.ToArray()).ToArray()
            );
            Structure.WorldObject.AddChildren(bags);
            
            /* Add Boat Merchants */
            PlaceNPCsWhenWorldReady(map[BoatMerchantColorCode], P => LoadBoatMerchants(P), Structure);
            
            /* Add Fisherman */
            PlaceNPCsWhenWorldReady(map[FishermanColorCode], P => LoadFishermans(P), Structure);
        }

        private static void PlaceNPCsWhenWorldReady(IEnumerable<VertexData> ScenePositions, Func<Vector3, IHumanoid[]> Load, CollidableStructure Structure)
        {
            var positions = ScenePositions.Select(V => V.AverageVertices()).ToArray();
            DoWhenWorldReady(positions, P => P, P =>
            {
                Structure.WorldObject.AddNPCs(Load(P));
            }, Structure);
        }
        
        private static void DoWhenWorldReady<T>(T[] Values, Func<T, Vector3> GetPosition, Action<T> Do, CollidableStructure Structure)
        {
            /* Add punching bags */
            for (var i = 0; i < Values.Length; ++i)
            {
                var k = i;
                DecorationsPlacer.PlaceWhenWorldReady(GetPosition(Values[i]), P => Do(Values[k]), () => Structure.Disposed);
            }
        }
        
        private static IHumanoid[] LoadFishermans(params Vector3[] Positions)
        {
            IHumanoid Transform(Vector3 Position)
            {
                var fisherman = World.WorldBuilding.SpawnHumanoid(HumanType.Fisherman, Position);
                fisherman.AddComponent(new QuestGiverComponent(fisherman, MissionPool.Random(Position, QuestTier.Medium)));
                fisherman.Physics.CollidesWithEntities = false;
                fisherman.IsSitting = true;
                fisherman.SearchComponent<DamageComponent>().Immune = true;
                return fisherman;
            }
            return Positions.Select(Transform).ToArray();
        }

        private static IHumanoid[] LoadBoatMerchants(params Vector3[] Positions)
        {
            IHumanoid Transform(Vector3 Position)
            {
                var boatMerchant = World.WorldBuilding.SpawnHumanoid(HumanType.Fisherman, Position);
                boatMerchant.SearchComponent<DamageComponent>().Immune = true;
                boatMerchant.AddComponent(new BoatMerchantComponent(boatMerchant));
                return boatMerchant;
            }
            return Positions.Select(Transform).ToArray();
        }

        private static BaseStructure[] LoadWells(params Vector3[][] VertexGroups)
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

        private static BaseStructure[] LoadPunchingBags(params Vector3[][] VertexGroups)
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