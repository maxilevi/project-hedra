using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class DesignTemplate
    {
        public string Path { get; set; }
        public string LodPath { get; set; }
        public Vector3 Offset { get; set; }
        public float Scale { get; set; } = 1;
    }
}