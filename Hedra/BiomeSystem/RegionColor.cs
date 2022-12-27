/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 04:31 p.m.
 *
 */

using System;
using System.Numerics;
using Hedra.Numerics;
using SixLabors.ImageSharp;

namespace Hedra.BiomeSystem
{
    /// <summary>
    ///     Description of Region.
    /// </summary>
    public class RegionColor
    {
        public static Vector4 PathColor = Color.Sienna.AsVector4();
        public static Vector4 DarkStonePathColor = Color.DimGray.AsVector4();
        private readonly int _seed;
        public Vector4 DirtColor;
        public Vector4[] GrassColors;
        public Vector4[] LeavesColors;
        public Vector4 SandColor;
        public Vector4 SeafloorColor;
        public Vector4 StoneColor;
        public Vector4 WaterColor;
        public Vector4[] WoodColors;
        public Vector4[] StonePathColors;

        public RegionColor(int Seed)
        {
            _seed = Seed;
        }

        public RegionColor(int Seed, BiomeColorsDesign Design)
        {
            _seed = Seed;
            BuildColors(Design);
            IntegrityCheck();
        }

        public Vector4 LeavesColor => LeavesColors[new Random(_seed + 42).Next(0, LeavesColors.Length)];

        public Vector4 WoodColor => WoodColors[new Random(_seed + 12).Next(0, WoodColors.Length)];

        public Vector4 GrassColor => GrassColors[new Random(_seed + 54).Next(0, GrassColors.Length)];
        
        public Vector4 StonePathColor => StonePathColors?[new Random(_seed + 54).Next(0, StonePathColors.Length)] ?? Color.Gray.AsVector4();

        public static RegionColor Interpolate(params RegionColor[] RegionsColor)
        {
            var inverseSize = 1f / RegionsColor.Length;
            var newRegion = new RegionColor(RegionsColor[0]._seed)
            {
                GrassColors = new Vector4[RegionsColor[0].GrassColors.Length],
                WoodColors = new Vector4[RegionsColor[0].WoodColors.Length],
                LeavesColors = new Vector4[RegionsColor[0].LeavesColors.Length]
            };

            for (var i = 0; i < RegionsColor.Length; i++)
            {
                newRegion.WaterColor += RegionsColor[i].WaterColor * inverseSize;
                newRegion.DirtColor += RegionsColor[i].DirtColor * inverseSize;
                newRegion.SandColor += RegionsColor[i].SandColor * inverseSize;
                newRegion.SeafloorColor += RegionsColor[i].SeafloorColor * inverseSize;
                newRegion.StoneColor += RegionsColor[i].StoneColor * inverseSize;


                for (var j = 0; j < newRegion.GrassColors.Length; j++)
                    newRegion.GrassColors[j] += RegionsColor[i].GrassColors[j] * inverseSize;

                for (var j = 0; j < newRegion.WoodColors.Length; j++)
                    newRegion.WoodColors[j] += RegionsColor[i].WoodColors[j] * inverseSize;

                for (var j = 0; j < newRegion.LeavesColors.Length; j++)
                    newRegion.LeavesColors[j] += RegionsColor[i].LeavesColors[j] * inverseSize;
            }

            return newRegion;
        }

        public void IntegrityCheck()
        {
            if (WaterColor == default) throw new ArgumentException("Region has invalid values.");
            if (StoneColor == default) throw new ArgumentException("Region has invalid values.");
            if (DirtColor == default) throw new ArgumentException("Region has invalid values.");
            if (SeafloorColor == default) throw new ArgumentException("Region has invalid values.");
            if (SandColor == default) throw new ArgumentException("Region has invalid values.");


            if (LeavesColors == null) throw new ArgumentException("Region has invalid values.");
            for (var i = 0; i < LeavesColors.Length; i++)
                if (LeavesColors[i] == default)
                    throw new ArgumentException("Region has invalid values.");

            if (WoodColors == null) throw new ArgumentException("Region has invalid values.");
            for (var i = 0; i < WoodColors.Length; i++)
                if (WoodColors[i] == default)
                    throw new ArgumentException("Region has invalid values.");

            if (GrassColors == null) throw new ArgumentException("Region has invalid values.");
            for (var i = 0; i < GrassColors.Length; i++)
                if (GrassColors[i] == default)
                    throw new ArgumentException("Region has invalid values.");
        }

        public void BuildColors(BiomeColorsDesign Design)
        {
            DirtColor = Design.DirtColor(_seed);
            SandColor = Design.SandColor(_seed);
            SeafloorColor = Design.SeafloorColor(_seed);
            StoneColor = Design.StoneColor(_seed);
            WaterColor = Design.WaterColor(_seed);
            GrassColors = Design.GrassColors(_seed);
            LeavesColors = Design.LeavesColors(_seed);
            WoodColors = Design.WoodColors(_seed);
            StonePathColors = Design.StonePathColors(_seed);


            for (var i = 0; i < GrassColors.Length; i++) GrassColors[i] = GrassColors[i] * 1.25f;
        }
    }
}