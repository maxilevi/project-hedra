using System;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.SnowBiome
{
    public class SnowBiomeColorsDesign : BiomeColorsDesign
    {
        public override Vector4 WaterColor(int Seed)
        {
            var rng = new Random(Seed + 23);
            int colorN = rng.Next(0, 7);
            Vector4 waterColor;
            switch (colorN)
            {
                case 0:
                    waterColor = Colors.FromHtml("#05668D");
                    break;
                case 1:
                    waterColor = Colors.FromHtml("#028090");
                    break;
                case 2:
                    waterColor = Colors.FromHtml("#00A896");
                    break;
                case 3:
                    waterColor = Colors.FromHtml("#02C39A");
                    break;
                case 4:
                    waterColor = Colors.FromHtml("#008AC1");
                    break;
                case 5:
                    waterColor = Colors.FromHtml("#50c6c5");
                    break;
                case 6:
                    waterColor = Colors.FromHtml("#66b1b0");
                    break;
                case 7:
                    waterColor = Colors.FromHtml("#2E8B57");
                    break;
                default:
                    throw new ArgumentException("Region color does not exist");
            }

            return waterColor * new Vector4(1f, 1f, 1f, .7f);
        }

        public override Vector4 StoneColor(int Seed)
        {
            var rng = new Random(Seed + 632);

            var colorN = rng.Next(0, 10);//4
            switch (colorN)
            {
                case 0:
                    return Colors.FromHtml("#393939");
                case 1:
                    return Colors.FromHtml("#3C3C3C");
                case 2:
                    return Colors.FromHtml("#585858");
                case 3:
                    return Colors.FromHtml("#868686");
                case 4:
                    return Colors.FromHtml("#736F72");
                case 5:
                    return Colors.FromHtml("#B2B2B2");
                case 6:
                    return Colors.FromHtml("#C3BABA");
                case 7:
                    return Colors.FromHtml("#9A8F97");
                case 8:
                    return Colors.FromHtml("#E9E3E6");
                case 9:
                    return Colors.FromHtml("#808782");
                default:
                    throw new ArgumentException("Region color does not exist");
            }
        }

        public override Vector4 DirtColor(int Seed)
        {
            return Colors.FromArgb(255, 134, 94, 60);
        }

        public override Vector4 SeafloorColor(int Seed)
        {
            return this.DirtColor(Seed) * .6f;
        }

        public override Vector4 SandColor(int Seed)
        {
            var rng = new Random(World.Seed + 1231);
            int colorN = rng.Next(0, 8);

            switch (colorN)
            {
                case 0:
                    return Colors.FromHtml("#e7d1a2");
                case 1:
                    return Colors.FromHtml("#E1A95F");
                case 2:
                    return Colors.FromHtml("#C2B280");
                case 3:
                    return Colors.FromHtml("#C19A6B");
                case 4:
                    return Colors.FromHtml("#F4A460");
                case 5:
                    return Colors.FromHtml("#d8bb75");
                case 6:
                    return Colors.FromHtml("#c09a6b");
                case 7:
                    return Colors.FromHtml("#b6571d");
                default: throw new ArgumentException("Region color does not exist");
            }
        }

        public override Vector4[] LeavesColors(int Seed)
        {
            var rngGen = new Random(Seed + 6432);
            int rng = rngGen.Next(0, 10);
            switch (rng)
            {
                case 0://
                    return new[] { Colors.FromHtml("#fea61e"), Colors.FromHtml("#ff5f30"), Colors.FromHtml("#c1ba5d"), Colors.FromHtml("#ff4f14") };
                case 1://
                    return new[] { Colors.FromHtml("#5b6b4c"), Colors.FromHtml("#74bf2f"), Colors.FromHtml("#567539"), Colors.FromHtml("#cace77") };
                case 7://
                    return new[] { Colors.FromHtml("#ff4b30"), Colors.FromHtml("#ff851c"), Colors.FromHtml("#ecdf6f"), Colors.FromHtml("#ffdd00") };
                case 3://
                    return new[] { Colors.FromHtml("#636df0"), Colors.FromHtml("#dd9bff"), Colors.FromHtml("#7e4aad"), Colors.FromHtml("#5139db") };
                case 4://
                    return new[] { Colors.FromHtml("#538378"), Colors.FromHtml("#6b8239"), Colors.FromHtml("#a4aa52"), Colors.FromHtml("#93d05b") };
                case 5://
                    return new[] { Colors.FromHtml("#489cff"), Colors.FromHtml("#1f53c9"), Colors.FromHtml("#78bb90"), Colors.FromHtml("#efdbb3") };
                case 6://
                    return new[] { Colors.FromHtml("#ed3f49"), Colors.FromHtml("#ff5738"), Colors.FromHtml("#f16fb7"), Colors.FromHtml("#ff91a6") };
                case 2://
                    return new[] { Colors.FromHtml("#60a023"), Colors.FromHtml("#9DA666"), Colors.FromHtml("#4f7942"), Colors.FromHtml("#5e714b") };
                case 8://
                    return new[] { Colors.FromHtml("#568203"), Colors.FromHtml("#228B22"), Colors.FromHtml("#A9BA9D"), Colors.FromHtml("#8A9A5B") };
                case 9://
                    return new[] { Colors.FromHtml("#87A96B"), Colors.FromHtml("#568203"), Colors.FromHtml("#4A5D23"), Colors.FromHtml("#6c7c59") };

                default: throw new ArgumentException("Region color does not exist");
            }
        }

        public override Vector4[] WoodColors(int Seed)
        {
            var rngGen = new Random(Seed + 6432);
            int rng = rngGen.Next(0, 4);
            if (World.Seed == World.MenuSeed) rng = 1;
            switch (rng)
            {
                case 0://
                    return new[] { Colors.FromHtml("#2D262A"), Colors.FromHtml("#3c2116"), Colors.FromHtml("#37322e"), Colors.FromHtml("#343633") };
                case 1://
                    return new[] { Colors.FromHtml("#845839"), Colors.FromHtml("#4e442b"), Colors.FromHtml("#5c3c23"), Colors.FromHtml("#7d6445") };
                case 2://
                    return new[] { Colors.FromHtml("#E8E2B7"), Colors.FromHtml("#edd2bb"), Colors.FromHtml("#f7eaca"), Colors.FromHtml("#d6ffc6") };
                case 3://
                    return new[] { Colors.FromHtml("#b49694"), Colors.FromHtml("#c2c5ba"), Colors.FromHtml("#9f8170"), Colors.FromHtml("#dbefc6") };

                default: throw new ArgumentException("Region color does not exist");
            }
        }

        public override Vector4[] GrassColors(int Seed)
        {
            var rngGen = new Random(Seed + 52234);
            int rng = rngGen.Next(0, 5);
            if (World.Seed == World.MenuSeed || rngGen.Next(0, 5) == 1)
                rng = 0;

            switch (rng)
            {
                case 0://
                    return new[] { Colors.FromHtml("#689d0f"), Colors.FromHtml("#bac51f"), Colors.FromHtml("#598e0a"), Colors.FromHtml("#9ac45c") };
                case 1:
                    return new[] { Colors.FromHtml("#a49a16"), Colors.FromHtml("#838812"), Colors.FromHtml("#5c6f2a"), Colors.FromHtml("#9ebd42") };
                //case 2://
                //    return new[] { GraphicUtils.FromHtml("#ff9b84"), GraphicUtils.FromHtml("#f4802d"), GraphicUtils.FromHtml("#e9674f"), GraphicUtils.FromHtml("#eb9b5a") };
                case 2://
                    return new[] { Colors.FromHtml("#77b870"), Colors.FromHtml("#2d562e"), Colors.FromHtml("#59954f"), Colors.FromHtml("#3e6834") };
                case 3://
                    return new[] { Colors.FromHtml("#6a7229"), Colors.FromHtml("#5b6310"), Colors.FromHtml("#276832"), Colors.FromHtml("#679731") };
                //case 5://
                //    return new[] { GraphicUtils.FromHtml("#a8a026"), GraphicUtils.FromHtml("#e7dc4f"), GraphicUtils.FromHtml("#957423"), GraphicUtils.FromHtml("#ea8e27") };
                case 4://
                    return new[] { Colors.FromHtml("#3d4f16"), Colors.FromHtml("#2e5d08"), Colors.FromHtml("#68771c"), Colors.FromHtml("#659318") };
                //case 5://
                //    return new[] { GraphicUtils.FromHtml("#7299fe"), GraphicUtils.FromHtml("#218271"), GraphicUtils.FromHtml("#1d796a"), GraphicUtils.FromHtml("#29ac80") };
                default: throw new ArgumentException("Region color does not exist");
            }
        }
    }
}
