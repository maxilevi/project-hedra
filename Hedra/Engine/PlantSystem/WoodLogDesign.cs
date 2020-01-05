using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class WoodLogDesign : RockDesign
    {
        public override CacheItem Type => CacheItem.WoodLog;

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.WoodColor);
            return Data;
        }
    }
}