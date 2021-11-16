using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PlantSystem.Harvestables;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class MushroomDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Mushroom;

        protected override Item ItemCollect => ItemPool.Grab(ItemType.Mushroom);

        public override float Scale(Random Rng)
        {
            return 1.0f + Rng.NextFloat() * .25f;
        }
    }
}