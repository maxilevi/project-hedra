using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.BiomeSystem
{
    public class Region
    {
        public RegionColor Colors;
        public RegionTree Trees;
        public RegionStructure Structures;
        public RegionSky Sky;
        public RegionMob Mob;

        public static Region Interpolate(params Region[] Regions)
        {
            var reg = new Region
            {
                Trees = RegionTree.Interpolate(Regions.Select(Reg => Reg.Trees).ToArray()),
                Colors = RegionColor.Interpolate(Regions.Select(Reg => Reg.Colors).ToArray()),
                Structures = RegionStructure.Interpolate(Regions.Select(Reg => Reg.Structures).ToArray()),
                Sky = RegionSky.Interpolate(Regions.Select(Reg => Reg.Sky).ToArray()),
                Mob = RegionMob.Interpolate(Regions.Select(Reg => Reg.Mob).ToArray()),
            };

            return reg;
        }
    }
}
