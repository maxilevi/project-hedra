using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class BuildingDesignTemplate : DesignTemplate
    {
        public DoorTemplate[] Doors { get; set; }
        public BedTemplate[] Beds { get; set; }
        public LightTemplate[] Lights { get; set; } = new LightTemplate[0];
        public Vector3 LampPosition { get; set; }
        public bool HasLamp { get; set; } = true;
    }
}