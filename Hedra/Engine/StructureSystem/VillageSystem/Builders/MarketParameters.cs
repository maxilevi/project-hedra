using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketParameters : BuildingParameters
    {
        public float Size { get; } = 160;
        public float WellSize { get; } = 64;

        public override float GetSize(VillageCache Cache)
        {
            return Size;
        }
    }
}