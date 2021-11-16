using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public interface ILivableBuildingParameters : IBuildingParameters
    {
        BuildingDesignTemplate Design { get; }
        BlockType Type { get; }
        GroundworkType GroundworkType { get; }
    }
}