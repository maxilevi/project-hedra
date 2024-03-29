using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Items;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class CarrotDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Carrot;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Carrot);
    }
}