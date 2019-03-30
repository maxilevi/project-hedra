using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeStructureDesign : BiomeStructureDesign
    {
        public NormalBiomeStructureDesign()
        {
            AddDesign(new VillageDesign());
            AddDesign(new GraveyardDesign());
            AddDesign(new GiantTreeDesign());
            AddDesign(new TravellingMerchantDesign());
            AddDesign(new ObeliskDesign());
            AddDesign(new CampfireDesign());
            AddDesign(new BanditCampDesign());
            AddDesign(new WellDesign());
        }

        public override VillageType VillageType => VillageType.Woodland;
    }
}
