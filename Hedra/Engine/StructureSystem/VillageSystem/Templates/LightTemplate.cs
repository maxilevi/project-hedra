using System.Numerics;
using Hedra.Engine.Core;

namespace Hedra.Engine.StructureSystem.VillageSystem.Templates
{
    public class LightTemplate : IPositionable
    {
        public int Radius { get; set; } = 18;
        public bool Indoors { get; set; }
        public Vector3 Position { get; set; }
    }
}