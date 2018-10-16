namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public interface IDispersedPlacementDesigner
    {
        PlacementDesign CreateDesign();
        void FinishPlacements(PlacementDesign Design);
    }
}