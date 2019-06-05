using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class PeasDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Peas;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Peas);
    }
}