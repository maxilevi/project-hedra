using Newtonsoft.Json;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageTemplate
    {
        public string Name { get; set; }
        public BuildingTemplate House { get; set; }
        public BuildingTemplate Well { get; set; }
        public BuildingTemplate Stable { get; set; }
        public BlacksmithTemplate Blacksmith { get; set; }
        public BuildingTemplate Farm { get; set; }
        public BuildingTemplate Windmill { get; set; }

        public BuildingTemplate[] Buildings => new []{ House, Well, Stable, Blacksmith, Farm, Windmill};
    }
}