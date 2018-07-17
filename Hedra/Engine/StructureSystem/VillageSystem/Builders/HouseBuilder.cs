using System;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class HouseBuilder : Builder
    {
        public override void Place(BuildingParameters Parameters)
        {
            this.PlaceGroundwork(Parameters.Position, 48);
        }
    }
}