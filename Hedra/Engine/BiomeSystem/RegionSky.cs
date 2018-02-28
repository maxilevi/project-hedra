using OpenTK;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionSky
    {

        public RegionSky(int Seed, BiomeSkyDesign Design)
        {
            IsRaining = Design.IsRaining(Seed);
            AfternoonTop = Design.AfternoonTop(Seed);
            AfternoonBot = Design.AfternoonBot(Seed);
            NightTop = Design.NightTop(Seed);
            NightBot = Design.NightBot(Seed);
            SunriseTop = Design.SunriseTop(Seed);
            SunriseBot = Design.SunriseBot(Seed);
            MiddayTop = Design.MiddayTop(Seed);
            MiddayBot = Design.MiddayBot(Seed);
            MinLight = Design.MinLight(Seed);
            MaxLight = Design.MaxLight(Seed);
        }

        public Vector4 AfternoonTop { get; }
        public Vector4 AfternoonBot { get; }
        public Vector4 NightTop { get; }
        public Vector4 NightBot { get; }
        public Vector4 SunriseTop { get; }
        public Vector4 SunriseBot { get; }
        public Vector4 MiddayTop { get; }
        public Vector4  MiddayBot { get; }
        public float MinLight { get; }
        public float MaxLight { get; }

        public bool IsRaining { get; }

        public static RegionSky Interpolate(params RegionSky[] Regions)
        {
            return Regions[0];
        }
    }
}
