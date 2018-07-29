using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    internal class BlacksmithPlacer : Placer<BlacksmithParameters>
    {
        private readonly BlacksmithDesignTemplate[] _blacksmithDesigns;
        public BlacksmithPlacer(BlacksmithDesignTemplate[] Designs, Random Rng) : base(Designs, Rng)
        {
            _blacksmithDesigns = Designs;
        }

        public override BlacksmithParameters FromPoint(PlacementPoint Point)
        {
            return new BlacksmithParameters
            {
                Design = this.SelectRandom(_blacksmithDesigns),
                Position = Point.Position,
                Rng = Rng
            };
        }
    }
}