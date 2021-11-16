using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class GrassDesign : WeedDesign
    {
        public override CacheItem Type => CacheItem.Grass;

        protected override void ApplyPaint(NativeVertexData Data, Region Region, Random Rng)
        {
            var newColor = new Vector4((Region.Colors.GrassColor * 1.0f).Xyz(), 1);
            Data.Paint(newColor);
        }
    }
}