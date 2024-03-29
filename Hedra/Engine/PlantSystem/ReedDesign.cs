using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class ReedDesign : WeedDesign
    {
        public override CacheItem Type => CacheItem.Reed;

        protected override void ApplyPaint(NativeVertexData Data, Region Region, Random Rng)
        {
            var grassColor = Region.Colors.GrassColor;
            Data.Color(AssetManager.ColorCode0, new Vector4((grassColor * .75f).Xyz(), 1));
            Data.Color(AssetManager.ColorCode1, new Vector4((grassColor * 1.25f).Xyz(), 1));
        }
    }
}