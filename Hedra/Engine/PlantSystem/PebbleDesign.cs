using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class PebbleDesign : BerryBushDesign
    {
        public override CacheItem Type => CacheItem.Pebble;

        protected override Item ItemCollect => ItemPool.Grab(ItemType.Pebble);

        public override float Scale(Random Rng)
        {
            return .75f + Rng.NextFloat() * .75f;
        }

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.StoneColor);
            return Data;
        }
    }
}