namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageBuilder
    {
        private readonly HouseBuilder _houseBuilder;
        
        public void Build(VillagePlacement Placement)
        {
            _houseBuilder.Place();
        }
    }
}