namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillagePlacement
    {
        public VillageRoot Root { get; private set; }
        public VillageConfiguration Configuration { get; private set; }

        public VillagePlacement(VillageRoot Root, VillageConfiguration Configuration)
        {
            this.Root = Root;
            this.Configuration = Configuration;
        }
    }
}