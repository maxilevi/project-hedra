using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem
{
    public abstract class BiomeEnviromentDesign
    {
        public abstract PlacementDesign[] Designs { get; }
    }
}
