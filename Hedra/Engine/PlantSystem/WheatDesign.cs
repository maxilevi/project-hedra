using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class WheatDesign : WeedDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Wheat);

        protected override void ApplyPaint(Vector3 Position, VertexData Data, Region Region, Random Rng)
        {
            var newColor = new Vector4((Region.Colors.GrassColor * 1.25f).Xyz, 1);

            Data.Color(AssetManager.ColorCode1, WheatColor(Rng));
            Data.Color(AssetManager.ColorCode0, newColor);
        }

        private static Vector4 WheatColor(Random Rng)
        {
            switch (Rng.Next(0, 7))
            {
                case 0: return Colors.FromHtml("#BF4B42");
                case 1: return Colors.FromHtml("#FF6380");
                case 2: return Colors.FromHtml("#AA3D98");
                case 3: return Colors.FromHtml("#FF65F2");
                case 4: return Colors.FromHtml("#379B95");
                case 5: return Colors.FromHtml("#FFAD5A");
                case 6: return Colors.FromHtml("#f4deb3");
            }
            return Vector4.One;
        }
    }
}
