using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class NeighbourhoodWellBuilder : Builder<HouseParameters>
    {
        public NeighbourhoodWellBuilder(CollidableStructure Structure) : base(Structure)
        {
        }

        public override bool Place(HouseParameters Parameters, VillageCache Cache)
        {
            return true;
        }
    }
}