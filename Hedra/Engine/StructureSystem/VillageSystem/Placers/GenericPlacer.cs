using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class GenericPlacer : Placer<GenericParameters>
    {
        private readonly GenericDesignTemplate[] _designs;
        private readonly int _maxPlacements;
        private readonly GenericNPCSettings _settings;
        private int _placements;

        public GenericPlacer(GenericDesignTemplate[] Designs, Random Rng, int MaxPlacements,
            GenericNPCSettings Settings) : base(Designs, Rng)
        {
            _settings = Settings;
            _designs = Designs;
            _maxPlacements = MaxPlacements;
        }

        public override bool SpecialRequirements(PlacementPoint Point)
        {
            return _placements < _maxPlacements;
        }

        protected override GenericParameters FromPoint(PlacementPoint Point)
        {
            _placements++;
            return new GenericParameters
            {
                Design = SelectRandom(_designs),
                Position = Point.Position,
                Rng = Rng,
                NPCSettings = _settings
            };
        }
    }
}