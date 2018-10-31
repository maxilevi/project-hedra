using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketParameters : BuildingParameters
    {
        public float Size { get; set; } = 200;
        public float WellSize { get; set; } = 72;

        public override float GetSize(VillageCache Cache)
        {
            return Size;
        }
    }
}