using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    internal class FarmPlacer : Placer<FarmParameters>
    {
        private readonly DesignTemplate[] _windmillDesigns;
        
        public FarmPlacer(DesignTemplate[] Farms, DesignTemplate[] Windmills, Random Rng) : base(Farms, Rng)
        {
            _windmillDesigns = Windmills;
        }

        protected override FarmParameters FromPoint(PlacementPoint Point)
        {
            return new FarmParameters
            {
                Design = this.SelectRandom(this.Designs),
                WindmillDesign = this.SelectRandom(this._windmillDesigns),
                Position = Point.Position,
                HasWindmill = Rng.Next(0, 5) == 1,
                Rng = Rng
            };
        }
    }
}