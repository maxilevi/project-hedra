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
        private float _width;
        public BlacksmithBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(BlacksmithParameters Parameters, VillageCache Cache)
        {
            
            return PlaceGroundwork(Parameters.Position, (_width = this.ModelRadius(Parameters, Cache)) * .5f, BlockType.StonePath);
        }

        public override BuildingOutput Build(BlacksmithParameters Parameters, DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Design, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Cache, Parameters.Design.Doors, transformation, output);
            return output;
        }

        public override void Polish(BlacksmithParameters Parameters, VillageRoot Root, Random Rng)
        {
            if(Rng.Next(0, 3) != 1) return;
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            var human = SpawnHumanoid(
                HumanType.Blacksmith,
                Vector3.Zero
            );
            var blacksmithOffset = Vector3.TransformPosition(Parameters.Design.Blacksmith * Parameters.Design.Scale, transformation);
            var lampOffset = Vector3.TransformPosition(Parameters.Design.LampPosition * Parameters.Design.Scale, transformation);
            var newPosition = Parameters.Position + blacksmithOffset;
            human.Physics.TargetPosition = newPosition + Vector3.UnitY * Physics.HeightAtPosition(newPosition);    
            DecorationsPlacer.PlaceLamp(Parameters.Position + lampOffset, Structure, Root, _width, Rng);
        }
    }
}