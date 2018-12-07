using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class FarmPlacer : Placer<FarmParameters>
    {
        private readonly FarmTemplate _farm;
        private readonly WindmillDesignTemplate[] _windmillDesigns;
        private readonly FarmDesignTemplate[] _farmDesigns;
        
        public FarmPlacer(FarmTemplate Template, FarmDesignTemplate[] Farms, WindmillDesignTemplate[] Windmills, Random Rng) : base(Farms, Rng)
        {
            _farm = Template;
            _farmDesigns = Farms;
            _windmillDesigns = Windmills;
        }

        public override FarmParameters FromPoint(PlacementPoint Point)
        {
            return new FarmParameters
            {
                Design = SelectRandom(_farmDesigns),
                WindmillDesign = SelectRandom(this._windmillDesigns),
                PropDesign = SelectProp(_farm.PropDesigns),
                Position = Point.Position,
                HasWindmill = Rng.Next(0, 3) == 1,
                PropDesigns = _farm.PropDesigns,
                Rng = Rng
            };
        }
        
        private PropTemplate SelectProp(PropTemplate[] Templates)
        {
            var rng = Rng.NextFloat();
            var accum = 0f;
            for (var i = 0; i < Templates.Length; i++)
            {
                if (rng < Templates[i].Chance / 100f + accum)
                    return Templates[i];
                accum += Templates[i].Chance / 100f;
            }
            return null;
        }
    }
}