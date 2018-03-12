using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.BiomeSystem.LavaBiome
{
    public class LavaBiomeStructureDesign : BiomeStructureDesign
    {
        public LavaBiomeStructureDesign()
        {
            this.AddDesign(new VillageDesign());
            this.AddDesign(new GraveyardDesign());
            this.AddDesign(new GiantTreeDesign());
            this.AddDesign(new WoodenFortDesign());
            this.AddDesign(new TravellingMerchantDesign());
            this.AddDesign(new ObeliskDesign());
            this.AddDesign(new CampfireDesign());
        }
    }
}
