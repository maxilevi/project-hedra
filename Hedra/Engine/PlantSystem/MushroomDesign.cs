using System;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PlantSystem.Harvestables;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class MushroomDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Mushroom;

        public override float Scale(Random Rng) => 1.0f + Rng.NextFloat() * .25f;
        
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Mushroom);
    }
}