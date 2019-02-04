using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class BuildingDesignTemplate : DesignTemplate
    {
        public DoorTemplate[] Doors { get; set; }
        public BedTemplate[] Beds { get; set; }
        public Vector3 LampPosition { get; set; }
    }
}