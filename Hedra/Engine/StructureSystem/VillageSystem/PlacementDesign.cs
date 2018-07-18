using Hedra.Engine.StructureSystem.VillageSystem.Builders;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class PlacementDesign
    {
        public FarmParameters[] Farms { get; set; } = new FarmParameters[0];
        public BuildingParameters[] Houses { get; set; } = new BuildingParameters[0];
        public BlacksmithParameters[] Blacksmith { get; set; } = new BlacksmithParameters[0];
        public BuildingParameters[] Stables { get; set; } = new BuildingParameters[0];
        public BuildingParameters[] Markets { get; set; } = new BuildingParameters[0];
    }
}