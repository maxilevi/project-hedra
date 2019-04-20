using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeStructureDesign : BiomeStructureDesign
    {
        public NormalBiomeStructureDesign()
        {
            /* Default structures */
            AddDesign(new SpawnCampfireDesign());
            AddDesign(new SpawnVillageDesign());
            
            AddDesign(new VillageDesign());
            AddDesign(new GraveyardDesign());
            AddDesign(new GiantTreeDesign());
            AddDesign(new TravellingMerchantDesign());
            AddDesign(new ObeliskDesign());
            AddDesign(new CampfireDesign());
            AddDesign(new BanditCampDesign());
            AddDesign(new WellDesign());
            AddDesign(new GazeboDesign());
            AddDesign(new GhostTownPortalDesign());
        }

        public override VillageType VillageType => VillageType.Woodland;
    }
}
