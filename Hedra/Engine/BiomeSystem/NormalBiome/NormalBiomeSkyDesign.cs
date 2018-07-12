using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    internal class NormalBiomeSkyDesign : BiomeSkyDesign
    {
        public override Vector4 AfternoonTop(int Seed)
        {
            return Colors.FromHtml("#fa8b5f");
        }

        public override Vector4 AfternoonBot(int Seed)
        {
            return Colors.FromHtml("#f8cb95");
        }

        public override Vector4 NightTop(int Seed)
        {
            return Colors.FromHtml("#34326a");
        }

        public override Vector4 NightBot(int Seed)
        {
            return Colors.FromHtml("#445c93");
        }

        public override Vector4 SunriseTop(int Seed)
        {
            return Colors.FromHtml("#8f8da1");
        }

        public override Vector4 SunriseBot(int Seed)
        {
            return Colors.FromHtml("#ebdde1");
        }

        public override Vector4 MiddayTop(int Seed)
        {
            return this.CanRain(Seed) ? CloudyTop : ClearTop;
        }

        public override Vector4 MiddayBot(int Seed)
        {
            return this.CanRain(Seed) ? CloudyBot : ClearBot;
        }

        private Vector4 CloudyTop { get; } = Colors.FromArgb(255, 51, 60, 57);
        private Vector4 CloudyBot { get; } = Colors.FromArgb(255, 253, 251, 240);
        private Vector4 ClearTop { get; } = Colors.DeepSkyBlue;
        private Vector4 ClearBot { get; } = Colors.FromArgb(255, 253, 251, 187);

        public override bool CanRain(int Seed)
        {
             return new Random(Seed + 2343).Next(0, 4) == 1;
        }

        public override float MinLight(int Seed)
        {
            return 0.15f;
        }

        public override float MaxLight(int Seed)
        {
            return float.MaxValue;
        }
    }
}
