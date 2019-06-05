using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class OnionDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Onion;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Onion);
    }
}