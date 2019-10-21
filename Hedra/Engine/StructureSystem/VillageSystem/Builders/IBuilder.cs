using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public interface IBuilder<out T> where T : IBuildingParameters
    {
        BuildingOutput Paint(BuildingOutput Input);

        BuildingOutput Build(DesignTemplate Design, VillageCache Cache, Random Rng, Vector3 Center);
        
        void Polish(Random Rng);
    }
}