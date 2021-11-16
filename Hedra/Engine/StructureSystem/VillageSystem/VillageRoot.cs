using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class VillageRoot
    {
        private VillageRoot()
        {
        }

        public VillageCache Cache { get; private set; }
        public VillageTemplate Template { get; private set; }

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