using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class DecorationsTemplate
    {
        public LampDesignTemplate[] Lamps { get; set; }
        public BenchDesignTemplate[] Benches { get; set; }
    }

    public class LampDesignTemplate : DesignTemplate
    {
        public Vector3 LightOffset { get; set; }
    }

    public class BenchDesignTemplate : DesignTemplate
    {
        public Vector3 SitOffset { get; set; }
    }
}