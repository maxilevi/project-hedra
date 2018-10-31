using Hedra.Engine.Generation;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public interface IPlacementDesigner
    {
        PlacementDesign CreateDesign();
        void FinishPlacements(CollidableStructure Structure, PlacementDesign Design);
    }
}