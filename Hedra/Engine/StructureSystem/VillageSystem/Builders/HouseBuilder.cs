using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseBuilder : Builder<HouseParameters>
    {
        protected override bool LookAtCenter => true;
        protected override bool GraduateColor => false;
        private float _width;
        
        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(HouseParameters Parameters, VillageCache Cache)
        {
            _width = Parameters.GetSize(Cache) * 2f;
            var ground = new RoundedGroundwork(Parameters.Position, _width * .5f * .75f, Parameters.Type)
            {
                NoPlants = true,
                NoTrees = true
            };
            var plateau = CreatePlateau(Parameters);
            return PushGroundwork(new GroundworkItem
            {
                Groundwork = ground,
                Plateau = IsPlateauNeeded(plateau) ? plateau : null
            });
        
        }

        private BasePlateau CreatePlateau(HouseParameters Parameters)
        {
            return GroundworkType.Squared == Parameters.GroundworkType
                ? (BasePlateau) new SquaredPlateau(Parameters.Position.Xz, _width) { Hardness = 3.0f }
                : new RoundedPlateau(Parameters.Position.Xz, _width * .5f * 1.5f) { Hardness = 3.0f };
        }

        public override void Polish(HouseParameters Parameters, VillageRoot Root, Random Rng)
        {
            var position = Parameters.Position + Vector3.TransformPosition(Vector3.UnitX * _width,
                               Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            
            if (Rng.Next(0, 4) == 1)
            {
                var villager = SpawnVillager(position, Rng);
                if (Utils.Rng.Next(0, 5) == 1)
                {
                    villager.RemoveComponent(villager.SearchComponent<TalkComponent>());
                    villager.RemoveComponent(villager.SearchComponent<ThoughtsComponent>());
                    villager.AddComponent(
                        new QuestGiverComponent(villager, QuestPool.Grab(QuestTier.Easy).Build(villager.Position, Utils.Rng, villager))
                    );
                }
            }
            else if (Rng.Next(0, 6) == 1)
            {
                SpawnMob(MobType.Pug, position);
            }

            var width = VillageDesign.Spacing * .5f;
            var offset = Vector3.TransformPosition(- width * .5f * Vector3.UnitZ - width * .5f * Vector3.UnitX,
                Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            DecorationsPlacer.PlaceLamp(Parameters.Position + offset, Structure, Root, _width, Rng);
        }

        public override BuildingOutput Build(HouseParameters Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Design, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Cache, Parameters.Design.Doors, transformation, output);
            AddBeds(Parameters, Parameters.Design.Beds, transformation, output);
            return output;
        }
    }
}