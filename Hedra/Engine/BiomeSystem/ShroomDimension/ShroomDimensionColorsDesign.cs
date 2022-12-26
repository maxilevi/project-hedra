using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.BiomeSystem.ShroomDimension
{
    public class ShroomDimensionColorsDesign : BiomeColorsDesign
    {
        public override Vector4 WaterColor(int Seed)
        {
            var rng = new Random(Seed + 23);
            var colorN = rng.Next(0, 7);
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

            return waterColor * new Vector4(.5f, .5f, .5f, .7f);
        }

        public override Vector4 StoneColor(int Seed)
        {
            var rngGen = new Random(Seed + 52234);

            var returnArray = WoodColors(Seed + 424423);
            for(var i = 0; i < returnArray.Length; i++)
            {
                var shade = 0.8f + 0.125f * (rngGen.NextFloat() * 2 - 1f);
                returnArray[i] *= new Vector4(shade, shade, shade, 1);
            }

            return returnArray[1];
        }

        public override Vector4 DirtColor(int Seed)
        {
            var rngGen = new Random(Seed + 52234);

            var returnArray = WoodColors(Seed + 424423);
            for(var i = 0; i < returnArray.Length; i++)
            {
                var shade = 0.8f + 0.125f * (rngGen.NextFloat() * 2 - 1f);
                returnArray[i] *= new Vector4(shade, shade, shade, 1);
            }

            return returnArray[0];
        }

        public override Vector4 SeafloorColor(int Seed)
        {
            return DirtColor(Seed) * .6f;
        }

        public override Vector4 SandColor(int Seed)
        {
            var rng = new Random(World.Seed + 1231);
            var colorN = rng.Next(0, 8);

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
            var rng = new Random(World.Seed + 534324);
            var colors =  new[]
            {
                Colors.FromHtml("#BC3431"),
                Colors.FromHtml("#5aabaf"),
                Colors.FromHtml("#ff1e25"),
                Colors.FromHtml("#2b2b23"),
                //Colors.FromHtml("#fe9d41"),
                Colors.FromHtml("#4327af"),
            };
            return new[] { colors[rng.Next(0, colors.Length)], colors[rng.Next(0, colors.Length)] };
        }

        public override Vector4[] WoodColors(int Seed)
        {
            var rng = new Random(World.Seed + 34463);
            var colors = new[]
            {
                Colors.FromHtml("#3E2E26"),
                //Colors.FromHtml("#ceb982"),
                Colors.FromHtml("#6d6e53"),
                Colors.FromHtml("#777b60"),
                Colors.FromHtml("#f19a3f"),
                Colors.FromHtml("#7a8db5"),
            };
            return new[] { colors[rng.Next(0, colors.Length)], colors[rng.Next(0, colors.Length)]  };
        }

        public override Vector4[] GrassColors(int Seed)
        {
            var rngGen = new Random(Seed + 52234);

            var returnArray = LeavesColors(Seed + 424423);
            for(var i = 0; i < returnArray.Length; i++)
            {
                var shade = 0.65f + 0.125f * (rngGen.NextFloat() * 2 - 1f);
                returnArray[i] *= new Vector4(shade, shade, shade, 1);
            }

            return returnArray;
        }
    }
}