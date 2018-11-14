using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class BlacksmithPlacer : Placer<BlacksmithParameters>
    {
        private int _currentBlacksmiths;
        private readonly BlacksmithDesignTemplate[] _blacksmithDesigns;
        public BlacksmithPlacer(BlacksmithDesignTemplate[] Designs, Random Rng) : base(Designs, Rng)
        {
            _blacksmithDesigns = Designs;
        }

        public override bool SpecialRequirements(PlacementPoint Point)
        {
            if (_currentBlacksmiths >= 3) return false;
            return true;
        }

        public override BlacksmithParameters FromPoint(PlacementPoint Point)
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