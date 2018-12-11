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
            var ground = new RoundedGroundwork(Parameters.Position, _width * .5f, Parameters.Type)
            {
                NoPlants = true,
                NoTrees = true
            };
            return PushGroundwork(new GroundworkItem
            {
                Groundwork = ground,
                Plateau = GroundworkType.Squared == Parameters.GroundworkType
                    ? (BasePlateau) new SquaredPlateau(Parameters.Position, _width) { Hardness = 3.0f }
                    : new RoundedPlateau(Parameters.Position, _width * .5f * 1.5f) { Hardness = 6.0f }                 
            });
        
        }

        public override void Polish(HouseParameters Parameters, Random Rng)
        {
            var position = Parameters.Position + Vector3.TransformPosition(Vector3.UnitX * _width,
                               Matrix4.CreateRotationY(Parameters.Rotation.Y * Mathf.Radian));
            
            if (Rng.Next(0, 8) == 1)
            {
                var villager = SpawnVillager(position, false);
                villager.AddComponent(new VillagerThoughtsComponent(villager));
            }
            else if (Rng.Next(0, 6) == 1)
            {
                var pug = SpawnMob(MobType.Pug, position);
            }
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