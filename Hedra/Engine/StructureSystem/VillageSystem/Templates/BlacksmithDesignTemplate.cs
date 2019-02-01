using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class BlacksmithDesignTemplate : BuildingDesignTemplate
    {
        public Vector3 Blacksmith { get; set; }
        public bool HasAnvil { get; set; }
        public bool HasWorkbench { get; set; }
        public Vector3 WorkbenchPosition { get; set; }
        public Vector3 AnvilPosition { get; set; }
    }
}