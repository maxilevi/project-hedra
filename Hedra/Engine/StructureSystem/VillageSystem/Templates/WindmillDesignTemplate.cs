using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class WindmillDesignTemplate : DesignTemplate, IProbabilityTemplate
    {
        public string BladesPath { get; set; }
        public Vector3 BladesPosition { get; set; }
        public int Chance { get; set; }
    }
}