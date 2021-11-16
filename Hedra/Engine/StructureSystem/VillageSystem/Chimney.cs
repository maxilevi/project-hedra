using System.Numerics;
using Hedra.Engine.StructureSystem.Overworld;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Chimney : Campfire
    {
        public Chimney(Vector3 Position) : base(Position)
        {
        }

        public override bool CanCraft => true;
    }
}