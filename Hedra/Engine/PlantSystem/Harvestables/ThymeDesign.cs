using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Items;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class ThymeDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Thyme;

        protected override Item ItemCollect => ItemPool.Grab(ItemType.Thyme);
    }
}