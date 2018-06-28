using System;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    internal class UndeadBiomeSkyDesign : BiomeSkyDesign
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
            return ClearTop;
        }

        public override Vector4 MiddayBot(int Seed)
        {
            return ClearBot;
        }

        private Vector4 ClearTop { get; } = Colors.FromHtml("#2f8e62");
        private Vector4 ClearBot { get; } = Colors.FromHtml("#2f5e62");

        public override bool CanRain(int Seed)
        {
            return false;
        }

        public override float MinLight(int Seed)
        {
            return 0.4f;
        }

        public override float MaxLight(int Seed)
        {
            return 0.8f;
        }
    }
}
