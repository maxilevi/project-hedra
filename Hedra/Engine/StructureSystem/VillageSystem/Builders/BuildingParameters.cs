using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class BuildingParameters : IBuildingParameters
    {
        public DesignTemplate Design { get; set; }
        public Vector3 Position { get; set; }
        public Random Rng { get; set; }
    }
}