using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class LightTemplate
    {
        public Vector3 Position { get; set; }
        public int Radius { get; set; } = 48;
        public bool Indoors { get; set; }
    }
}