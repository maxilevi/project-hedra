using Hedra.Engine.Core;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class LightTemplate : IPositionable
    {
        public Vector3 Position { get; set; }
        public int Radius { get; set; } = 18;
        public bool Indoors { get; set; }
    }
}