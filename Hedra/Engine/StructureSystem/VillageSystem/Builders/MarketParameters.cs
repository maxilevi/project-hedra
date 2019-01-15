using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketParameters : BuildingParameters
    {
        public const float MarketSize = 108;
        
        public float Size { get; } = MarketSize;
        public float WellSize { get; } = 18;

        public override float GetSize(VillageCache Cache)
        {
            return Size;
        }
    }
}