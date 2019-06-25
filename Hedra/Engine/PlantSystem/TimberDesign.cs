using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Items;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class TimberDesign : BerryBushDesign
    {
        public override CacheItem Type => CacheItem.Timber;

        public override float Scale(Random Rng) => .75f + Rng.NextFloat() * .75f;

        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.WoodColor);
            return Data;
        }

        protected override Item ItemCollect => ItemPool.Grab(ItemType.Timber);
    }
}