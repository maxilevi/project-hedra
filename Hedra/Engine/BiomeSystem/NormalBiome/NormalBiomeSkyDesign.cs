using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeSkyDesign : BiomeSkyDesign
    {
        public override Vector4 AfternoonTop(int Seed)
        {
            return Colors.FromHtml("#8f5543");
        }

        public override Vector4 AfternoonBot(int Seed)
        {
            return Colors.FromHtml("#f78d59");
        }

        public override Vector4 NightTop(int Seed)
        {
            return Colors.FromHtml("#09021b");
        }

        public override Vector4 NightBot(int Seed)
        {
            return Colors.FromHtml("#0f042e");
        }

        public override Vector4 SunriseTop(int Seed)
        {
            return Colors.FromHtml("#b64360");
        }

        public override Vector4 SunriseBot(int Seed)
        {
            return Colors.FromHtml("#fdc0ab");
        }

        public override Vector4 MiddayTop(int Seed)
        {
            return this.IsRaining(Seed) ? CloudyTop : ClearTop;
        }

        public override Vector4 MiddayBot(int Seed)
        {
            return this.IsRaining(Seed) ? CloudyBot : ClearBot;
        }

        private Vector4 CloudyTop { get; } = Colors.FromArgb(255, 51, 60, 57);
        private Vector4 CloudyBot { get; } = Colors.FromArgb(255, 253, 251, 240);
        private Vector4 ClearTop { get; } = Colors.DeepSkyBlue;
        private Vector4 ClearBot { get; } = Colors.FromArgb(255, 253, 251, 187);

        public override bool IsRaining(int Seed)
        {
             return new Random(Seed + 2343).Next(0, 4) == 1;
        }

        public override float MinLight(int Seed)
        {
            return float.MinValue;
        }

        public override float MaxLight(int Seed)
        {
            return float.MaxValue;
        }
    }
}
