using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class BlacksmithPlacer : Placer<BlacksmithParameters>
    {
        private readonly BlacksmithDesignTemplate[] _blacksmithDesigns;
        private readonly int _maxPlacements;
        private int _currentBlacksmiths;

        public BlacksmithPlacer(BlacksmithDesignTemplate[] Designs, Random Rng, int MaxPlacements) : base(Designs, Rng)
        {
            _maxPlacements = MaxPlacements;
            _blacksmithDesigns = Designs;
        }

        public override bool SpecialRequirements(PlacementPoint Point)
        {
            // if (_currentBlacksmiths >= _maxPlacements) return false;
            return true;
        }

        protected override BlacksmithParameters FromPoint(PlacementPoint Point)
        {
            _currentBlacksmiths++;
            return new BlacksmithParameters
            {
                Design = SelectRandom(_blacksmithDesigns),
                Position = Point.Position,
                Rng = Rng
            };
        }
    }
}