using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class HousePlacer : NeighbourhoodPlacer
    {
        public HousePlacer(DesignTemplate[] Designs, DesignTemplate[] WellDesigns, Random Rng) : base(Designs, WellDesigns, Rng)
        {
        }
        
        public override NeighbourhoodParameters FromPoint(PlacementPoint Point)
        {
            var parameter = base.FromPoint(Point);
            parameter.IsSingle = true;
            return parameter;
        }
    }
}