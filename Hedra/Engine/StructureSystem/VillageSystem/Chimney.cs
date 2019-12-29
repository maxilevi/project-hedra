using Hedra.Engine.StructureSystem.Overworld;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Chimney : Campfire
    {
        public override bool CanCraft => false;
        
        public Chimney(Vector3 Position) : base(Position)
        {
        }
    }
}