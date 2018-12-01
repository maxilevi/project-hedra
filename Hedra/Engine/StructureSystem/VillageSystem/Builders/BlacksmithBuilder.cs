using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BlacksmithBuilder : Builder<BlacksmithParameters>
    {
        public BlacksmithBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(BlacksmithParameters Parameters, VillageCache Cache)
        {
            return PlaceGroundwork(Parameters.Position, this.ModelRadius(Parameters, Cache) * .5f, BlockType.StonePath);
        }

        public override BuildingOutput Build(BlacksmithParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Parameters.Design.Doors, transformation, output);
            return output;
        }

        public override void Polish(BlacksmithParameters Parameters)
        {
            SpawnHumanoid(HumanType.Blacksmith, Parameters.Position + Parameters.Design.Blacksmith * Parameters.Design.Scale);
        }
    }
}