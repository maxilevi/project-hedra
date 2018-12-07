namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class VillageTemplate
    {
        public string Name { get; set; }
        public HouseTemplate House { get; set; }
        public WellTemplate Well { get; set; }
        public StableTemplate Stable { get; set; }
        public BlacksmithTemplate Blacksmith { get; set; }
        public FarmTemplate Farm { get; set; }
        public WindmillTemplate Windmill { get; set; }

        public DesignTemplate[][] CacheableDesigns => new []
        {
            House.Designs,
            Well.Designs,
            Stable.Designs,
            Farm.Designs,
            Farm.PropDesigns,
            Windmill.Designs,
            Blacksmith.Designs
        };
    }
}