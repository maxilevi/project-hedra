namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class VillagePlacement
    {
        public VillagePlacement(VillageRoot Root, VillageConfiguration Configuration)
        {
            this.Root = Root;
            this.Configuration = Configuration;
        }

        public VillageRoot Root { get; }
        public VillageConfiguration Configuration { get; }
    }
}