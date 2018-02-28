using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionMob
    {
        public RegionMob(int Seed, BiomeMobDesign Design)
        {
            
        }

        public static RegionMob Interpolate(params RegionMob[] Regions)
        {
            return Regions[0];
        }
    }
}
