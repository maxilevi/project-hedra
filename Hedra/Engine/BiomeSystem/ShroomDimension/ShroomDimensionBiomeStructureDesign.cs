using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Structures;

namespace Hedra.Engine.BiomeSystem.ShroomDimension
{
    public class ShroomDimensionBiomeStructureDesign : BiomeStructureDesign
    {
        public ShroomDimensionBiomeStructureDesign()
        {
            AddDesign(new SpawnShroomDimensionPortalDesign());
            //AddDesign(new ShroomDimensionBossDesign());
            //AddDesign(new GraveyardDesign());
            //AddDesign(new GiantTreeDesign());
            AddDesign(new TravellingMerchantDesign());
            AddDesign(new ObeliskDesign());
            AddDesign(new CampfireDesign());
            //AddDesign(new WitchHutDesign());
        }

        public override VillageType VillageType => VillageType.None;
    }
}