using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class NeighbourhoodParameters : BuildingParameters
    {
        public float Size { get; set; }
        public DesignTemplate[] HouseTemplates { get; set; }
        public int HouseCount { get; set; }

        public override float GetSize(VillageCache Cache)
        {
            return Size * .5f;
        }
    }
}