using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.BiomeSystem
{
    public class Region
    {
        public RegionColor Colors { get; set; }
        public RegionTree Trees{ get; set; }
        public RegionStructure Structures { get; set; }
        public RegionSky Sky { get; set; }
        public RegionMob Mob { get; set; }
        public RegionGeneration Generation { get; set; }
        public RegionEnviroment Enviroment { get; set; }

        public static Region Interpolate(params Region[] Regions)
        {
            var reg = new Region
            {
                Trees = RegionTree.Interpolate(Regions.Select(Reg => Reg.Trees).ToArray()),
                Colors = RegionColor.Interpolate(Regions.Select(Reg => Reg.Colors).ToArray()),
                Structures = RegionStructure.Interpolate(Regions.Select(Reg => Reg.Structures).ToArray()),
                Sky = RegionSky.Interpolate(Regions.Select(Reg => Reg.Sky).ToArray()),
                Mob = RegionMob.Interpolate(Regions.Select(Reg => Reg.Mob).ToArray()),
                Generation = RegionGeneration.Interpolate(Regions.Select(Reg => Reg.Generation).ToArray()),
                Enviroment = RegionEnviroment.Interpolate(Regions.Select(Reg => Reg.Enviroment).ToArray()),
            };

            return reg;
        }
    }
}
