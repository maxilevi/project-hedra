using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PlantSystem.Harvestables;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushDesign : HarvestableDesign
    {
        public override CacheItem Type => CacheItem.BerryBush;
        protected override Item ItemCollect => ItemPool.Grab(ItemType.Berry);
        
        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            Data = base.Paint(Data, Region, Rng);
            Data.Color(AssetManager.ColorCode1, Colors.BerryColor(Rng));
            return Data;
        }
    }
}