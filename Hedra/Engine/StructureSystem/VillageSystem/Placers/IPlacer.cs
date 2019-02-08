using Hedra.Engine.StructureSystem.VillageSystem.Builders;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public interface IPlacer<out T> where T : IBuildingParameters
    {
        T Place(PlacementPoint Point);

        bool SpecialRequirements(PlacementPoint Point);
    }
}