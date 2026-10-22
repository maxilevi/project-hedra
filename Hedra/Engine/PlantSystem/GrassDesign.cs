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

        public override PreparedPlantPaint PreparePaint(Region Region, Random Rng)
        {
            if (GetType() != typeof(GrassDesign)) return null;
            var color = new Vector4((Region.Colors.GrassColor * 1.0f).Xyz(), 1);
            // Keep the legacy two model selections, including their RNG consumption.
            var highest = PreparedPlantPaint.Bounds.Get(Model).Highest;
            var lowest = PreparedPlantPaint.Bounds.Get(Model).Lowest;
            return new PreparedPlantPaint.Grass(color, lowest, Vector3.Dot(highest - lowest, Vector3.UnitY));
        }

        protected override void ApplyPaint(NativeVertexData Data, Region Region, Random Rng)
        {
            var newColor = new Vector4((Region.Colors.GrassColor * 1.0f).Xyz(), 1);
            Data.Paint(newColor);
        }
    }
}
