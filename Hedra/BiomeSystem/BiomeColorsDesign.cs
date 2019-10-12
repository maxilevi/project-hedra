using OpenToolkit.Mathematics;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeColorsDesign
    {
        public abstract Vector4 WaterColor(int Seed);
        public abstract Vector4 StoneColor(int Seed);
        public abstract Vector4 DirtColor(int Seed);
        public abstract Vector4 SeafloorColor(int Seed);
        public abstract Vector4 SandColor(int Seed);
        public abstract Vector4[] LeavesColors(int Seed);
        public abstract Vector4[] WoodColors(int Seed);
        public abstract Vector4[] GrassColors(int Seed);
    }
}
