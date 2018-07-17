using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal interface IBuildingParameters
    {
        DesignTemplate Design { get; set; }
        Vector3 Position { get; set; }
        Random Rng { get; set; }
    }
}