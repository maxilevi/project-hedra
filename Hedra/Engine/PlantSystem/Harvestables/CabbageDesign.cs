using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class CabbageDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Cabbage;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Cabbage);
    }
}