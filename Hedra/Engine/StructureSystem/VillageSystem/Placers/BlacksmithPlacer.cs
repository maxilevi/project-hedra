using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class BlacksmithPlacer : Placer<BlacksmithParameters>
    {
        private int _currentBlacksmiths;
        private readonly int _maxPlacements;
        private readonly BlacksmithDesignTemplate[] _blacksmithDesigns;
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
                Design = this.SelectRandom(_blacksmithDesigns),
                Position = Point.Position,
                Rng = Rng
            };
        }
    }
}