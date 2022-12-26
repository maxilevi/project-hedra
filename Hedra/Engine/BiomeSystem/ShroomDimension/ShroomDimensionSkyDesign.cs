using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Rendering;

namespace Hedra.Engine.BiomeSystem.ShroomDimension
{
    public class ShroomDimensionSkyDesign : BiomeSkyDesign
    {

        public override Vector4 AfternoonTop(int Seed)
        {
            return NightTop(Seed);
        }

        public override Vector4 AfternoonBot(int Seed)
        {
            return NightBot(Seed);
        }

        public override Vector4 NightTop(int Seed)
        {
            return Colors.FromHtml("#3a435a");
        }

        public override Vector4 NightBot(int Seed)
        {
            return Colors.FromHtml("#4b5e58");
        }

        public override Vector4 SunriseTop(int Seed)
        {
            return MiddayTop(Seed);
        }

        public override Vector4 SunriseBot(int Seed)
        {
            return MiddayBot(Seed);
        }

        public override Vector4 MiddayTop(int Seed)
        {
            return Colors.FromHtml("#3b4c54");
        }

        public override Vector4 MiddayBot(int Seed)
        {
            return Colors.FromHtml("#b4edff");
        }

        public override bool CanRain(int Seed)
        {
            return false;//true;
        }

        public override float MinLight(int Seed)
        {
            return 0.4f;
        }

        public override float MaxLight(int Seed)
        {
            return 0.6f;
        }
    }
}