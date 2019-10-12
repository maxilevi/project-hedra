using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.PlantSystem
{
    public class GrassDesign : WeedDesign
    {
        public override CacheItem Type => CacheItem.Grass;

        protected override void ApplyPaint(NativeVertexData Data, Region Region, Random Rng)
        {
            var newColor = new Vector4((Region.Colors.GrassColor * 1.0f).Xyz, 1);
            Data.Paint(newColor);
        }
    }
}
