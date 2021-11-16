using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Items;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class PeasDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.Peas;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Peas);

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode1, Region.Colors.GrassColor * 1.25f);
            Data.Color(AssetManager.ColorCode0, Region.Colors.GrassColor * 1.5f);
            return Data;
        }
    }
}