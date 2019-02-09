using Hedra.Engine.Core;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class LightTemplate : IPositionable
    {
        public Vector3 Position { get; set; }
        public int Radius { get; set; } = 48;
        public bool Indoors { get; set; }
    }
}