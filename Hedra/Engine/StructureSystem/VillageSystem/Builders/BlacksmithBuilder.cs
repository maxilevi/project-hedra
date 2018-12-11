using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
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

        public override BuildingOutput Build(BlacksmithParameters Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Design, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Cache, Parameters.Design.Doors, transformation, output);
            return output;
        }

        public override void Polish(BlacksmithParameters Parameters, Random Rng)
        {
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            var human = SpawnHumanoid(
                HumanType.Blacksmith,
                Vector3.Zero
            );
            var newPosition = Parameters.Position +
                              Vector3.TransformPosition(Parameters.Design.Blacksmith * Parameters.Design.Scale,
                                  transformation);
            human.Physics.TargetPosition = newPosition + Vector3.UnitY * Physics.HeightAtPosition(newPosition);
            human.Physics.UsePhysics = false;
        }
    }
}