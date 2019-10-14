using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class DoorTemplate
    {
        public string Path { get; set; }
        public Vector3 Position { get; set; }
        public bool InvertedPivot { get; set; }
        public bool InvertedRotation { get; set; } = true;
    }
}