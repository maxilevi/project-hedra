using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PlantSystem.Harvestables;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.BerryBush;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Berry);
    }
}