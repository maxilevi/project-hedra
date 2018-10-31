using Hedra.Engine.EntitySystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem
{
    public class GiantTree : BaseStructure
    {
        public IEntity Boss { get; set; }

        public override void Dispose()
        {
            Boss?.Dispose();
            base.Dispose();
        }
    }
}