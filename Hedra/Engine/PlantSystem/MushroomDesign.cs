using System;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class MushroomDesign : BerryBushDesign
    {
        public override CacheItem Type => CacheItem.Mushroom;

        protected override float Scale(Random Rng) => 1.0f + Rng.NextFloat() * .25f;

        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            Data.Colors = Data.Colors.Select(C => C * 1.5f).ToList();
            return Data;
        }

        protected override Item ItemCollect => ItemPool.Grab(ItemType.Mushroom);
    }
}