using System;
using System.Numerics;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public interface IBuildingParameters
    {
        DesignTemplate Design { get; set; }
        Vector3 Position { get; set; }
        Vector3 Rotation { get; set; }
        Random Rng { get; set; }
        float GetSize(VillageCache Cache);
    }
}