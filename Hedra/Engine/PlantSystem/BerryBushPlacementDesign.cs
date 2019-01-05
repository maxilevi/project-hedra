using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushPlacementDesign : CollectibleDesign
    {
        protected override PlantDesign Design { get; } = new BerryBushDesign();
        
        protected override BlockType Type => BlockType.Grass;

        protected override bool ShouldPlace(Random Rng) => Rng.Next(0, 3000) == 1;
    }
}
