using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class ReedDesign : WeedDesign
    {
        protected override CacheItem Type => CacheItem.Reed;

        protected override void ApplyPaint(VertexData Data, Region Region, Random Rng)
        {
            var grassColor = Region.Colors.GrassColor;
            Data.Color(AssetManager.ColorCode0, new Vector4((grassColor * .75f).Xyz, 1));
            Data.Color(AssetManager.ColorCode1, new Vector4((grassColor * 1.25f).Xyz, 1));
        }
    }
}