using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketParameters : BuildingParameters
    {
        public float Size { get; } = 108;
        public float WellSize { get; } = 15;

        public override float GetSize(VillageCache Cache)
        {
            return Size;
        }
    }
}