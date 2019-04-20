using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem.DesertBiome
{
    public class DesertBiomeStructureDesign : BiomeStructureDesign
    {
        public DesertBiomeStructureDesign()
        {
            this.AddDesign(new VillageDesign());
            this.AddDesign(new GraveyardDesign());
            this.AddDesign(new GiantTreeDesign());
            this.AddDesign(new TravellingMerchantDesign());
            this.AddDesign(new ObeliskDesign());
            this.AddDesign(new CampfireDesign());
        }

        public override VillageType VillageType => VillageType.Desert;
    }
}
