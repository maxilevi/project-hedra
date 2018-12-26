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
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            if (Rng.Next(0, 3) == 1)
            {
                var blacksmithOffset = Vector3.TransformPosition(Parameters.Design.Blacksmith * Parameters.Design.Scale, transformation);
                var newPosition = Parameters.Position + blacksmithOffset;
                DecorationsPlacer.PlaceWhenWorldReady(newPosition,
                    P =>
                    {
                        var human = SpawnHumanoid(
                            HumanType.Blacksmith,
                            P
                        );
                        human.Physics.TargetPosition = P;
                    }
                );
            }
            var lampOffset = Vector3.TransformPosition(Parameters.Design.LampPosition * Parameters.Design.Scale, transformation);
            DecorationsPlacer.PlaceLamp(Parameters.Position + lampOffset, Structure, Root, _width, Rng);
            if (Parameters.Design.HasAnvil)
            {
                var anvilPosition = Parameters.Position + Vector3.TransformPosition(
                                        Parameters.Design.AnvilPosition * Parameters.Design.Scale,
                                        transformation);
                DecorationsPlacer.PlaceWhenWorldReady(anvilPosition,
                    P => Structure.WorldObject.AddChildren(new Anvil(P))
                );
            }
        }
    }
}