using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class NeighbourhoodPlacer : Placer<NeighbourhoodParameters>
    {
        protected DesignTemplate[] WellDesigns { get; set; }
        
        public NeighbourhoodPlacer(DesignTemplate[] Designs, DesignTemplate[] WellDesigns, Random Rng) : base(Designs, Rng)
        {
            this.WellDesigns = WellDesigns;
        }
        
        public override NeighbourhoodParameters FromPoint(PlacementPoint Point)
        {
            return new NeighbourhoodParameters
            {
                Design = WellDesigns[Rng.Next(0, WellDesigns.Length)],
                HouseTemplates = Designs,
                Position = Point.Position,
                Rng = Rng,
                Size = 160 + Rng.NextFloat() * 32,
                HouseCount = 4 + Rng.Next(0, 4)
            };
        }
    }
}