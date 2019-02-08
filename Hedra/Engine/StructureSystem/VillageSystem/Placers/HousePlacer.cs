using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class HousePlacer : Placer<HouseParameters>
    {
        protected DesignTemplate[] WellDesigns { get; set; }
        private readonly HouseDesignTemplate[] _houseDesigns;
        
        public HousePlacer(HouseDesignTemplate[] Designs, DesignTemplate[] WellDesigns, Random Rng) : base(Designs, Rng)
        {
            this.WellDesigns = WellDesigns;
            _houseDesigns = Designs;
        }

        protected override HouseParameters FromPoint(PlacementPoint Point)
        {
            return new HouseParameters
            {
                Design = this.SelectRandom(_houseDesigns),
                WellTemplate = WellDesigns[Rng.Next(0, WellDesigns.Length)],
                Position = Point.Position,
                Rng = Rng
            };
        }
    }
}