using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class VillageBuilder
    {
        private readonly HouseBuilder _houseBuilder;
        private readonly FarmBuilder _farmBuilder;
        private readonly BlacksmithBuilder _blacksmithBuilder;
        private readonly StableBuilder _stableBuilder;
        private readonly WellBuilder _wellBuilder;
        private readonly MarketBuilder _marketBuilder;
        private readonly PlacementDesigner _designer; 

        public VillageBuilder()
        {
            _houseBuilder = new HouseBuilder();
            _farmBuilder = new FarmBuilder();
            _blacksmithBuilder = new BlacksmithBuilder();
            _stableBuilder = new StableBuilder();
            _wellBuilder = new WellBuilder();
            _marketBuilder = new MarketBuilder();
            _designer = new PlacementDesigner();
        }

        public PlacementDesign DesignVillage()
        {
            return _designer.CreateDesign();
        }
        
        public void PlaceGroundwork(PlacementDesign Design)
        {
            if (Design.HouseCount > 0)
            {
                _houseBuilder.Place(this.BuildParameters);
            }
            if (Design.FarmCount > 0)
            {
                _farmBuilder.Place(this.BuildParameters);
            }
            if (Design.BlacksmithCount > 0)
            {
                _blacksmithBuilder.Place(this.BuildParameters);
            }
            if (Design.StableCount > 0)
            {
                _stableBuilder.Place(this.BuildParameters);
            }
            if (Design.WellCount > 0)
            {
                _wellBuilder.Place(this.BuildParameters);
                //_marketBuilder.Place(this.BuildParameters);
            }
        }

        public void Build(PlacementDesign Design)
        {
            
        }

        private T BuildParameters<T>() where T : BuildingParameters, new()
        {
            return new T
            {
                
            };
        }
    }
}