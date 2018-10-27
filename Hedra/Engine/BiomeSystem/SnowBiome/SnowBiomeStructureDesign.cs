using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem.SnowBiome
{
    public class SnowBiomeStructureDesign : BiomeStructureDesign
    {
        public SnowBiomeStructureDesign()
        {
            this.AddDesign(new VillageDesign());
            this.AddDesign(new GraveyardDesign());
            this.AddDesign(new GiantTreeDesign());
            this.AddDesign(new WoodenFortDesign());
            this.AddDesign(new TravellingMerchantDesign());
            this.AddDesign(new ObeliskDesign());
            this.AddDesign(new CampfireDesign());
        }

        public override VillageType VillageType => VillageType.Nordic;
    }
}
