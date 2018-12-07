namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class FarmTemplate : BuildingTemplate<FarmDesignTemplate>
    {
        public PropTemplate[] PropDesigns { get; set; }
    }
            
    public class PropTemplate : DesignTemplate
    {
        public int Chance { get; set; }
        public bool HasLivestock { get; set; }
        public bool HasWindmill { get; set; }
        public int WindmillChance { get; set; }
    }
}