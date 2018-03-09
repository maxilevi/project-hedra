using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionEnviroment
    {
        private readonly BiomeEnviromentDesign _design;
        private readonly int _seed;

        public RegionEnviroment(int Seed, BiomeEnviromentDesign Design)
        {
            this._seed = Seed;
            this._design = Design;
        }

        public PlacementDesign[] Designs => _design.Designs;

        public static RegionEnviroment Interpolate(params RegionEnviroment[] Regions)
        {
            return Regions[0];
        }
    }
}
