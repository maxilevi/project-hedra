using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    internal class MarketPlacer : Placer<BuildingParameters>
    {
        private int _currentMarkets;

        public MarketPlacer(DesignTemplate[] Designs, Random Rng) : base(Designs, Rng)
        {
        }

        protected override bool SpecialRequirements(PlacementPoint Point)
        {
            if (_currentMarkets >= 1) return false;
            _currentMarkets++;
            Point.Position = Vector3.Zero;
            return true;
        }
    }
}
