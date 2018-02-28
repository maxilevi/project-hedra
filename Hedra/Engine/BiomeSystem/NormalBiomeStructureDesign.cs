using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.BiomeSystem
{
    public class NormalBiomeStructureDesign : BiomeStructureDesign
    {
        public NormalBiomeStructureDesign()
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
