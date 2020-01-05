using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class TreeStumpDesign : RockDesign
    {
        public override CacheItem Type => CacheItem.TreeStump;

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.WoodColor);
            return Data;
        }
    }
}