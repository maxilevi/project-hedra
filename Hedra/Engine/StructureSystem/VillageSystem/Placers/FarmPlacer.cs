using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class FarmPlacer : Placer<FarmParameters>
    {
        private readonly DesignTemplate[] _windmillDesigns;
        private readonly FarmDesignTemplate[] _farmDesigns;
        
        public FarmPlacer(FarmDesignTemplate[] Farms, DesignTemplate[] Windmills, Random Rng) : base(Farms, Rng)
        {
            _farmDesigns = Farms;
            _windmillDesigns = Windmills;
        }

        public override FarmParameters FromPoint(PlacementPoint Point)
        {
            return new FarmParameters
            {
                Design = SelectRandom(_farmDesigns),
                WindmillDesign = SelectRandom(this._windmillDesigns),
                Position = Point.Position,
                HasWindmill = Rng.Next(0, 3) == 1,
                Rng = Rng
            };
        }
    }
}