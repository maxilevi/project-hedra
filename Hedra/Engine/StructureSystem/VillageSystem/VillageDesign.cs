namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageDesign
    {
        public VillageCache Cache { get; private set; }
        public VillageTemplate Template { get; private set; }
        
        private VillageDesign()
        {
        }

        public static VillageDesign FromTemplate(VillageTemplate Template)
        {
            return new VillageDesign
            {
                Cache = VillageCache.FromTemplate(Template),
                Template = Template
            };
        }
    }
}