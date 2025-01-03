using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.ShroomDimension;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Structures;

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
            AddDesign(new CampfireWithQuestDesign());
            AddDesign(new BanditCampDesign());
            AddDesign(new WellDesign());
            AddDesign(new GazeboDesign());
            AddDesign(new WitchHutDesign());
            AddDesign(new FishingPostDesign());
            AddDesign(new WizardTowerDesign());
            AddDesign(new UndeadDungeon0Design());
            AddDesign(new UndeadDungeon1Design());
            AddDesign(new UndeadDungeon2Design());
            AddDesign(new CottageWithFarmDesign());
            AddDesign(new CottageWithVegetablePlotDesign());
            AddDesign(new GhostTownPortalDesign());
            AddDesign(new SolitaryFishermanDesign());
            AddDesign(new GarrisonDesign());
            AddDesign(new GnollFortressDesign());
            AddDesign(new Cave0Design());
            AddDesign(new Cave1Design());
            AddDesign(new Cave2Design());
            AddDesign(new Cave3Design());
            AddDesign(new Cave4Design());
            AddDesign(new Cave5Design());
            AddDesign(new Cave6Design());
            AddDesign(new ShroomDimensionPortalDesign());
            
        }

        public override VillageType VillageType => VillageType.Woodland;
    }
}