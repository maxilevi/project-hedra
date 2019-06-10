using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class StickDesign : BerryBushDesign
    {
        public override CacheItem Type => CacheItem.Stick;

        public override float Scale(Random Rng) => .75f + Rng.NextFloat() * .75f;

        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.WoodColor * 1.25f);
            return Data;
        }

        protected override Item ItemCollect => ItemPool.Grab(ItemType.WoodenStick);
    }
}