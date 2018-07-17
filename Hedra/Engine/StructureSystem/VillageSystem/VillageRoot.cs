using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageRoot
    {
        public VillageCache Cache { get; private set; }
        public VillageTemplate Template { get; private set; }
        
        private VillageRoot()
        {
        }

        public static VillageRoot FromTemplate(VillageTemplate Template)
        {
            return new VillageRoot
            {
                Cache = VillageCache.FromTemplate(Template),
                Template = Template
            };
        }
    }
}