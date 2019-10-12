using OpenToolkit.Mathematics;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeSkyDesign
    {
        public abstract Vector4 MiddayTop(int Seed);
        public abstract Vector4 MiddayBot(int Seed);
        public abstract Vector4 AfternoonTop(int Seed);
        public abstract Vector4 AfternoonBot(int Seed);
        public abstract Vector4 NightTop(int Seed);
        public abstract Vector4 NightBot(int Seed);
        public abstract Vector4 SunriseTop(int Seed);
        public abstract Vector4 SunriseBot(int Seed);

        public abstract bool CanRain(int Seed);
        public abstract float MaxLight(int Seed);
        public abstract float MinLight(int Seed);
    }
}
