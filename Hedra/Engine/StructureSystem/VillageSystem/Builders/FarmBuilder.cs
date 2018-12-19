using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.AISystem;
using Hedra.AISystem.Humanoid;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class FarmBuilder : Builder<FarmParameters>
    {
        private float _width;
        protected override bool LookAtCenter => false;
        protected override bool GraduateColor => false;
        
        public FarmBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override BuildingOutput Build(FarmParameters Parameters, DesignTemplate Template, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = new BuildingOutput
            {
                Models = new VertexData[0],
                LodModels = null,
                TransformationMatrices = new Matrix4[0],
                Shapes = new List<CollisionShape>(),
                GraduateColors = GraduateColor
            };
            Parameters.Rotation = Vector3.UnitY * Rng.Next(0, 4) * 90;
            SpawnPlants(Parameters, Rng, output);
            if (Parameters.PropDesign != null)
            {
                var prop = base.Build(Parameters, Parameters.PropDesign, Cache, Rng, Center);
                if (Parameters.PropDesign.HasWindmill)
                {
                    if (Rng.NextFloat() > Parameters.PropDesign.WindmillChance / 100f) return output.Concat(prop);
                    var windmill = BuildWindmill(Parameters, Parameters.PropDesign, Cache, Rng, Center);
                    prop = prop.Concat(windmill);
                }
                return output.Concat(prop);
            }
            return output;
        }

        private BuildingOutput BuildWindmill(FarmParameters Parameters, DesignTemplate Template, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var windmill = base.Build(Parameters, Parameters.WindmillDesign, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddBlades(Parameters, Cache, transformation, windmill);
            return windmill;
        }

        private void AddBlades(FarmParameters Parameters, VillageCache Cache, Matrix4 Transformation, BuildingOutput Output)
        {
            var template = Parameters.WindmillDesign;
            var vertexData = Cache.GetOrCreate(template.BladesPath, Vector3.One * template.Scale);
            vertexData.Center();
            vertexData.Transform(Transformation);
            var shapes = Cache.GetOrCreateShapes(template.BladesPath, Vector3.One * template.Scale);
            if(shapes.Count > 1)
                throw new ArgumentOutOfRangeException($"The number of shapes for the windmill blades must be 1 but it's '{shapes.Count}'");
            shapes.ForEach(S => S.Transform(Transformation));
            var offset = Vector3.TransformPosition(template.BladesPosition * template.Scale, Transformation);
            Output.Structures.Add(
                new WindmillBlades(
                    vertexData,
                    shapes.FirstOrDefault(),
                    Vector3.TransformPosition(Vector3.UnitX, Transformation),
            Parameters.Position + offset,
                    Structure
                )
            );        
        }
        
        public override void Polish(FarmParameters Parameters, VillageRoot Root, Random Rng)
        {
            var dir = Vector3.TransformPosition(Vector3.UnitZ, Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            if (Rng.Next(0, 4) == 1)
            {
                var position = Parameters.Position + dir * _width * .5f;
                SpawnFarmer(position, Parameters.Position.Xz);
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

        private void SpawnPlants(FarmParameters Parameters, Random Rng, BuildingOutput Output)
        {
            var count = Rng.Next(11, 24);
            PlantDesign design = null;
            var rng = Rng.NextFloat();
            
            if (rng < .6f)
                design = new GrassDesign();
            else if (rng < .8f)
                design = new FernDesign();
            
            if(design == null) return;
            var added = new List<Vector3>();
            for (var x = 0; x < count * count; x++)
            {
                var offset = new Vector3((Rng.NextFloat() * 2f - 1f) * _width * .5f, 0,
                    (Rng.NextFloat() * 2f - 1f) * _width * .5f);
                var position = Parameters.Position + offset;
                const float minDist = Chunk.BlockSize * 2.5f;
                if (added.Any(P => (P - position).LengthSquared < minDist * minDist)) continue;
                var transMatrix = Matrix4.CreateScale(6.0f + Utils.Rng.NextFloat() * .5f)
                                  * Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat() * Mathf.Radian)
                                  * Matrix4.CreateTranslation(position);
                added.Add(position);
                Output.Instances.Add(BuildPlant(design.Model, design, World.BiomePool.GetRegion(position), transMatrix, Rng));
            }
        }

        private InstanceData BuildPlant(VertexData Model, PlantDesign Design, Region Biome, Matrix4 Transformation, Random Rng)
        {
            var modelClone = Model.Clone();
            Design.Paint(modelClone, Biome, Rng);
            var data = new InstanceData
            {
                OriginalMesh = Model,
                Colors = modelClone.Colors.Clone(),
                ExtraData = modelClone.Extradata.Clone(),
                HasExtraData = true,
                VariateColor = true,
                GraduateColor = true,
                TransMatrix = Transformation,
                PlaceCondition = B => B == BlockType.FarmDirt
            };
            CacheManager.Check(data);
            return data;
        }
        
        private void SpawnFarmer(Vector3 Position, Vector2 FarmPosition)
        {
            var farmer = SpawnVillager(Position);
            farmer.SetHelmet(ItemPool.Grab(CommonItems.FarmerHat).Helmet);
            farmer.SetWeapon(ItemPool.Grab(CommonItems.FarmingRake).Weapon);
            farmer.AddComponent(new FarmerAIComponent(farmer, FarmPosition, Vector2.One * _width));
            farmer.AddComponent(new FarmerThoughtsComponent(farmer));
        }
        
        public override bool Place(FarmParameters Parameters, VillageCache Cache)
        {
            _width = Parameters.GetSize(Cache);
            var path = new SquaredGroundwork(Parameters.Position, _width, BlockType.FarmDirt)
            {
                BonusHeight = .35f
            };
            var plateau = new SquaredPlateau(Parameters.Position, _width);
            return this.PushGroundwork(new GroundworkItem
            {
                Groundwork = path,
                Plateau = IsPlateauNeeded(plateau) ? plateau : null
            });
        }
    }
}