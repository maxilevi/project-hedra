using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class PumpkinDesign : FarmPlantDesign
    {
        public override CacheItem Type => CacheItem.Pumpkin;
        
        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, new Vector4((Region.Colors.GrassColor * 1.25f).Xyz(), 1));
            Data.Color(AssetManager.ColorCode1, new Vector4((Region.Colors.GrassColor * .75f).Xyz(), 1));
            return Data;
        }
    }
}