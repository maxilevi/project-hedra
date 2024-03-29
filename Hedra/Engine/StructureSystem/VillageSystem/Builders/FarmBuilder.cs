using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.AISystem;
using Hedra.AISystem.Humanoid;
using Hedra.AISystem.Mob;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Framework;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class FarmBuilder : Builder<FarmParameters>
    {
        private float _width;

        public FarmBuilder(CollidableStructure Structure) : base(Structure)
        {
        }

        protected override bool LookAtCenter => false;
        protected override bool GraduateColor => false;

        public override BuildingOutput Build(FarmParameters Parameters, DesignTemplate Template, VillageCache Cache,
            Random Rng, Vector3 VillageCenter)
        {
            var output = new BuildingOutput
            {
                Models = new VertexData[0],
                LodModels = null,
                TransformationMatrices = new Matrix4x4[0],
                Shapes = new List<CollisionShape>(),
                GraduateColors = GraduateColor
            };
            Parameters.Rotation = Vector3.UnitY * Rng.Next(0, 4) * 90;
            SpawnPlants(Parameters, Rng, output);
            if (Parameters.PropDesign != null && (!Parameters.PropDesign.OnlyOnOutskirts || !Parameters.InsidePaths))
            {
                var prop = base.Build(Parameters, Parameters.PropDesign, Cache, Rng, VillageCenter);
                if (Parameters.PropDesign.HasWindmill)
                {
                    if (Rng.NextFloat() > Parameters.PropDesign.WindmillChance / 100f) return output.Concat(prop);
                    var windmill = BuildWindmill(Parameters, Parameters.PropDesign, Cache, Rng, VillageCenter);
                    prop = prop.Concat(windmill);
                }

                return output.Concat(prop);
            }

            return output;
        }

        private BuildingOutput BuildWindmill(FarmParameters Parameters, DesignTemplate Template, VillageCache Cache,
            Random Rng, Vector3 Center)
        {
            var windmill = base.Build(Parameters, Parameters.WindmillDesign, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            if (Parameters.WindmillDesign.BladesPath != null) AddBlades(Parameters, Cache, transformation, windmill);
            return windmill;
        }

        private void AddBlades(FarmParameters Parameters, VillageCache Cache, Matrix4x4 Transformation,
            BuildingOutput Output)
        {
            var template = Parameters.WindmillDesign;
            var vertexData = Cache.GetOrCreate(template.BladesPath, Vector3.One * template.Scale);
            vertexData.Center();
            vertexData.Transform(Transformation);
            var shapes = Cache.GetOrCreateShapes(template.BladesPath, Vector3.One * template.Scale);
            if (shapes.Count > 1)
                throw new ArgumentOutOfRangeException(
                    $"The number of shapes for the windmill blades must be 1 but it's '{shapes.Count}'");
            shapes.ForEach(S => S.Transform(Transformation));
            var offset = Vector3.Transform(template.BladesPosition * template.Scale, Transformation);
            Output.Structures.Add(
                new WindmillBlades(
                    vertexData,
                    shapes.FirstOrDefault(),
                    Vector3.Transform(Vector3.UnitZ, Transformation),
                    Parameters.Position + offset,
                    Structure
                )
            );
        }

        public override void Polish(FarmParameters Parameters, VillageRoot Root, Random Rng)
        {
            var dir = Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            if (Rng.Next(0, 3) == 1)
            {
                var position = Parameters.Position + dir * _width * .5f;
                SpawnFarmer(position, Parameters.Position.Xz(), Rng);
            }
            else if (Rng.Next(0, 5) == 1)
            {
                var type = MobType.None;
                var rng = Rng.NextFloat();
                if (rng < .2f)
                    type = MobType.Sheep;
                else if (rng < .5f)
                    type = MobType.Pig;
                else if (rng < 1)
                    type = MobType.Cow;
                var count = Rng.Next(1, 4);
                for (var i = 0; i < count; ++i)
                {
                    var position = Parameters.Position + dir * new Vector3(Rng.NextFloat() * _width - _width * .5f, 0,
                        Rng.NextFloat() * _width - _width * .5f);
                    var animal = SpawnMob(type, position);
                    animal.RemoveComponent(animal.SearchComponent<BasicAIComponent>());
                    switch (type)
                    {
                        case MobType.Cow:
                            animal.AddComponent(new CowFarmAIComponent(animal, Parameters.Position, _width));
                            break;
                        case MobType.Sheep:
                            animal.AddComponent(new SheepFarmAIComponent(animal, Parameters.Position, _width));
                            break;
                        case MobType.Pig:
                            animal.AddComponent(new PigFarmAIComponent(animal, Parameters.Position, _width));
                            break;
                    }
                }
            }
        }

        private unsafe void SpawnPlants(FarmParameters Parameters, Random Rng, BuildingOutput Output)
        {
            var count = Rng.Next(11, 24);
            PlantDesign design = null;
            var type = ItemType.MaxEnums;
            var rng = Rng.NextFloat();
            var rotModifier = 1;
            var minDistModifier = 2.5f;

            if (rng < .3f)
            {
                design = new PumpkinDesign();
                type = ItemType.Pumpkin;
            }
            else if (rng < .6f)
            {
                design = new CornDesign();
                minDistModifier = 1.5f;
                type = ItemType.Corn;
            }
            else if (rng < .75f)
            {
                design = new SunflowerDesign();
                rotModifier = 0;
            }

            if (design == null) return;
            var added = new List<Vector3>();
            var size = Allocator.Kilobyte * 256;
            var mem = stackalloc byte[size];
            using (var allocator = new StackAllocator(size, mem))
            {
                for (var x = 0; x < count * count; x++)
                {
                    var offset = new Vector3((Rng.NextFloat() * 2f - 1f) * _width * .5f, 0,
                        (Rng.NextFloat() * 2f - 1f) * _width * .5f);
                    var position = Parameters.Position + offset;
                    var minDist = Chunk.BlockSize * minDistModifier;
                    if (added.Any(P => (P - position).LengthSquared() < minDist * minDist)) continue;
                    if (Parameters.InsidePaths && offset.LengthFast() > _width - VillageDesign.PathWidth * 4) continue;
                    var transMatrix = Matrix4x4.CreateScale(6.0f + Utils.Rng.NextFloat() * .5f)
                                      * Matrix4x4.CreateRotationY(
                                          360 * Utils.Rng.NextFloat() * Mathf.Radian * rotModifier)
                                      * Matrix4x4.CreateTranslation(position - Vector3.UnitY);
                    added.Add(position);
                    var model = design.Model;
                    var region = World.BiomePool.GetRegion(position);
                    Output.Instances.Add(BuildPlant(allocator, model, design, region, transMatrix, Rng));
                    if (type != ItemType.MaxEnums && Rng.Next(0, 8) == 1)
                        BuildPlantCollectible(allocator, type, model, design, region, transMatrix, Rng);
                }
            }
        }

        private void BuildPlantCollectible(IAllocator Allocator, ItemType Type, VertexData Model, PlantDesign Design,
            Region Biome, Matrix4x4 Transformation, Random Rng)
        {
            var partModel = CacheManager.GetPart(Design.Type, Model);
            var data = BuildPlant(Allocator, partModel, Design, Biome, Transformation, Rng, 1.5f);
            DecorationsPlacer.PlaceWhenWorldReady(data.Position, P =>
            {
                data.TransMatrix *= Matrix4x4.CreateTranslation(Vector3.UnitY * P.Y);
                Structure.WorldObject.AddChildren(new CollectibleObject(
                    data.Position,
                    data,
                    ItemPool.Grab(Type)
                ));
            }, () => Structure.Disposed);
        }

        private static InstanceData BuildPlant(IAllocator Allocator, VertexData Model, PlantDesign Design, Region Biome,
            Matrix4x4 Transformation, Random Rng, float ColorMultiplier = 1)
        {
            var modelClone = Model.NativeClone(Allocator);
            Design.Paint(modelClone, Biome, Rng);
            for (var i = 0; i < modelClone.Colors.Count; ++i) modelClone.Colors[i] *= ColorMultiplier;
            var data = new InstanceData
            {
                OriginalMesh = Model,
                Colors = modelClone.Colors,
                ExtraData = modelClone.Extradata,
                VariateColor = true,
                GraduateColor = true,
                SkipOnLod = Rng.Next(0, 3) == 1,
                TransMatrix = Transformation
            };
            CacheManager.Check(data);
            modelClone.Dispose();
            return data;
        }

        private void SpawnFarmer(Vector3 Position, Vector2 FarmPosition, Random Rng)
        {
            var farmer = SpawnHumanoid(HumanType.Farmer, Position);
            farmer.SearchComponent<HealthBarComponent>().Name = Translations.Get("farmer");
            farmer.SetWeapon(ItemPool.Grab(ItemType.FarmingRake).Weapon);
            farmer.AddComponent(new FarmerAIComponent(farmer, FarmPosition, Vector2.One * _width));
            farmer.AddComponent(new FarmerThoughtsComponent(farmer));
        }

        public override bool Place(FarmParameters Parameters, VillageCache Cache)
        {
            _width = Parameters.GetSize(Cache);
            var path = new SquaredGroundwork(Parameters.Position, _width, BlockType.FarmDirt)
            {
                BonusHeight = Parameters.BonusFarmHeight
            };
            var plateau = new SquaredPlateau(Parameters.Position.Xz(), _width)
            {
                Hardness = 3.0f
            };
            return PushGroundwork(new GroundworkItem
            {
                Groundwork = path,
                Plateau = IsPlateauNeeded(plateau) ? plateau : null
            });
        }
    }
}