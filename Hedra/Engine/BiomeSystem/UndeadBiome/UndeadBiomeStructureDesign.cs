﻿using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    internal class UndeadBiomeStructureDesign : BiomeStructureDesign
    {
        public UndeadBiomeStructureDesign()
        {
            this.AddDesign(new VillageDesign());
            this.AddDesign(new GraveyardDesign());
            this.AddDesign(new TravellingMerchantDesign());
            this.AddDesign(new ObeliskDesign());
            this.AddDesign(new CampfireDesign());
        }

        public override VillageScheme Scheme { get; } = new NormalVillageScheme();
    }
}
