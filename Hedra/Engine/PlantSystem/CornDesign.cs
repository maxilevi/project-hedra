using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class CornDesign : FarmPlantDesign
    {
        public override CacheItem Type => CacheItem.Corn;

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, new Vector4((Region.Colors.GrassColor * 1).Xyz(), 1));
            Data.Color(AssetManager.ColorCode1, new Vector4((Region.Colors.GrassColor * .5f).Xyz(), 1));
            return Data;
        }
    }
}