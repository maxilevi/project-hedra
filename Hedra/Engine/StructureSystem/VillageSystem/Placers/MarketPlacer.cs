using System;
using System.Numerics;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class MarketPlacer : Placer<MarketParameters>
    {
        private int _currentMarkets;

        public MarketPlacer(DesignTemplate[] Designs, Random Rng) : base(Designs, Rng)
        {
        }

        public override bool SpecialRequirements(PlacementPoint Point)
        {
            if (_currentMarkets >= 1) return false;
            return true;
        }

        protected override MarketParameters FromPoint(PlacementPoint Point)
        {
            Point.Position = Vector3.Zero;
            _currentMarkets++;
            return base.FromPoint(Point);
        }
    }
}