using Hedra.Engine.EntitySystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public sealed class GiantTree : BaseStructure
    {
        public IEntity Boss { get; set; }

        public GiantTree(Vector3 Position) : base(Position)
        {           
        }
        
        public override void Dispose()
        {
            Boss?.Dispose();
            base.Dispose();
        }
    }
}