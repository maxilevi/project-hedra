namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class FarmTemplate : BuildingTemplate<FarmDesignTemplate>
    {
        public PropTemplate[] PropDesigns { get; set; }
    }

    public class PropTemplate : DesignTemplate, IProbabilityTemplate
    {
        public bool HasLivestock { get; set; }
        public bool HasWindmill { get; set; }
        public bool OnlyOnOutskirts { get; set; }
        public int WindmillChance { get; set; }
        public int Chance { get; set; }
    }
}