using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class BuildingDesignTemplate : DesignTemplate
    {
        public DoorTemplate[] Doors { get; set; } = new DoorTemplate[0];
        public BedTemplate[] Beds { get; set; } = new BedTemplate[0];
        public LightTemplate[] Lights { get; set; } = new LightTemplate[0];
        public ChimneyTemplate[] Chimneys { get; set; } = new ChimneyTemplate[0];
        public StructureTemplate[] Structures { get; set; } = new StructureTemplate[0];
        public Vector3 LampPosition { get; set; }
        public bool HasLamp { get; set; } = true;
    }
}