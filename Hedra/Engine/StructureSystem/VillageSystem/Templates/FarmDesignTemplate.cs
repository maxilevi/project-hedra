using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class FarmDesignTemplate : DesignTemplate
    {
        public PlantTemplate[] Plants { get; set; }
    }
    
    public class PlantTemplate
    {
        public string Type { get; set; }
        public Vector3 Position { get; set; }
    }
}