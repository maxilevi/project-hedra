using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Items;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class TomatoDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Tomato;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Tomato);
    }
}