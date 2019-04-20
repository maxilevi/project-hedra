using Hedra.Engine.StructureSystem.Overworld;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class Chimney : Campfire
    {
        protected override bool CanCraft => false;
        
        public Chimney(Vector3 Position) : base(Position)
        {
        }
    }
}