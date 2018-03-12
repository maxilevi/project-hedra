using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    public class UndeadBiomeStructureDesign : BiomeStructureDesign
    {
        public UndeadBiomeStructureDesign()
        {
            this.AddDesign(new VillageDesign());
            this.AddDesign(new GraveyardDesign());
            this.AddDesign(new TravellingMerchantDesign());
            this.AddDesign(new ObeliskDesign());
            this.AddDesign(new CampfireDesign());
        }
    }
}
