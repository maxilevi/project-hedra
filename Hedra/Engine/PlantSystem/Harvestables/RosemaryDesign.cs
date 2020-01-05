using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering;
using Hedra.Items;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class RosemaryDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Rosemary;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Rosemary);
    }
}