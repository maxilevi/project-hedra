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
        public HouseBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(HouseParameters Parameters, VillageCache Cache)
        {
            var work = CreateGroundwork(Parameters.Position, Parameters.GetSize(Cache), BlockType.Grass);
            work.NoTrees = true;
            work.NoPlants = true;
            return this.PushGroundwork(work);
        }

        public override BuildingOutput Build(HouseParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var output = base.Build(Parameters, Cache, Rng, Center);
            var transformation = BuildTransformation(Parameters).ClearTranslation();
            AddDoors(Parameters, Parameters.Design.Doors, transformation, output);
            AddBeds(Parameters, Parameters.Design.Beds, transformation, output);
            return output;
        }
    }
}