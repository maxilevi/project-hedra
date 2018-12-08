using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class HouseBuilder : Builder<HouseParameters>
    {
        protected override bool LookAtCenter => true;
        protected override bool GraduateColor => false;
        
        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(HouseParameters Parameters, VillageCache Cache)
        {
            var width = Parameters.GetSize(Cache) * 2f;
            var ground = new SquaredGroundwork(Parameters.Position, width * .5f, Parameters.Type)
            {
                NoPlants = true,
                NoTrees = true
            };
            return PushGroundwork(new GroundworkItem
            {
                Groundwork = ground,
                Plateau = GroundworkType.Squared == Parameters.GroundworkType
                    ? (BasePlateau) new SquaredPlateau(Parameters.Position, width) { Hardness = 3.0f }
                    : new RoundedPlateau(Parameters.Position, width * .5f * 1.5f) { Hardness = 6.0f }                 
            });
        
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