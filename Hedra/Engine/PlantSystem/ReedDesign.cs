using System;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class ReedDesign : WeedDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Reed);

        protected override void ApplyPaint(Vector3 Position, VertexData Data, Region Region, Random Rng)
        {
            var grassColor = Region.Colors.GrassColor;
            Data.Color(AssetManager.ColorCode0, new Vector4((grassColor * .75f).Xyz, 1));
            Data.Color(AssetManager.ColorCode1, new Vector4((grassColor * 1.25f).Xyz, 1));
        }
    }
}