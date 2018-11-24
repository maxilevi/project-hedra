using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class BlacksmithDesignTemplate : DesignTemplate
    {
        public DoorTemplate[] Doors { get; set; }
        public Vector3 Blacksmith { get; set; }
    }
}