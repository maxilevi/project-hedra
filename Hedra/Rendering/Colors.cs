using System;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Globalization;
using Hedra.Core;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Rendering
{
    public static class Colors
    {
        public static Vector4 LowHealthRed = new Vector4(0.878f, 0.196f, 0.235f, 1);
        public static Vector4 FullHealthGreen = new Vector4(0.4f, 0.6627451f, 0.4f, 1);
        public static Vector4 LightBlue = new Vector4(0.200f, 0.514f, 0.714f, 1f);
        public static Vector4 Violet = new Vector4(0.941f, 0.008f, 0.608f, 1f);
        public static Vector4 PoisonGreen = new Vector4(0.282f, 0.725f, 0.373f, 1f);

        public static Vector4 DarkRed { get; } = new Vector4(.5f, 0, 0, 1);
        public static Vector4 Red { get; } = new Vector4(1, 0, 0, 1);
        public static Vector4 Sienna { get; } = new Vector4(0.625f, 0.3203125f, 0.17578125f, 1);
        public static Vector4 Gray { get; } = new Vector4(.5f, .5f, .5f, 1f);
        public static Vector4 DeepSkyBlue { get; } = new Vector4(0, 0.74609375f, 1, 1);
        public static Vector4 Transparent { get; } = new Vector4(0, 0, 0, 0);
        public static Vector4 Blue { get; } = new Vector4(0, 0, 1, 1);
        public static Vector4 Yellow { get; } = Color.Yellow.ToVector4();
        public static Vector4 White { get; } = Color.White.ToVector4();
        public static Vector4 BlueViolet { get; } = Color.BlueViolet.ToVector4();
        public static Vector4 OrangeRed { get; } = Color.OrangeRed.ToVector4();
        public static Vector4 GreenYellow { get; } = Color.GreenYellow.ToVector4();
        public static Vector4 CooldownBlue { get; } = new Vector4(.2f, .2f, .85f, 1);
        public static Vector4 Brown { get; } = Color.FromArgb(255, 134, 94, 60).ToVector4() * .75f;
        public static Vector4 Magenta { get; } = Color.Magenta.ToVector4();

        public static Color ToColorStruct(Vector4 Color)
        {
            return Color.ToColor();
        }

        public static Vector4 FromHtml(string Hex)
        {
            if (Hex == null) return default;
            var argb = int.Parse(Hex.Replace("#", string.Empty), NumberStyles.HexNumber);

            return FromArgb(255, //(byte)((argb & -16777216) >> 0x18),
                (byte)((argb & 0xff0000) >> 0x10),
                (byte)((argb & 0xff00) >> 8),
                (byte)(argb & 0xff));
        }

        public static Vector4 FromArgb(float A, float R, float G, float B)
        {
            return new Vector4(R, G, B, A);
        }

        public static Vector4 FromArgb(byte A, byte R, byte G, byte B)
        {
            var inverse = 1 / 256f;
            return new Vector4(R * inverse, G * inverse, B * inverse, A * inverse);
        }

        /// <summary>
        /// Converts HSL to RGB, with a specified output Alpha.
        /// Arguments are limited to the defined range:
        /// does not raise exceptions.
        /// </summary>
        /// <param name="h">Hue, must be in [0, 360].</param>
        /// <param name="s">Saturation, must be in [0, 1].</param>
        /// <param name="l">Luminance, must be in [0, 1].</param>
        /// <param name="a">Output Alpha, must be in [0, 255].</param>
        public static Vector4 HsLtoRgba(double h, double s, double l, double a)
        {
            h = Math.Max(0D, Math.Min(360D, h));
            s = Math.Max(0D, Math.Min(1D, s));
            l = Math.Max(0D, Math.Min(1D, l));
            a = Math.Max(0D, Math.Min(1D, a));

            if (Math.Abs(s) < 0.000000000000001) return FromArgb((float)a, (float)l, (float)l, (float)l);

            var q = l < .5D
                ? l * (1D + s)
                : l + s - l * s;
            var p = 2D * l - q;

            var hk = h / 360D;
            var T = new double[3];
            T[0] = hk + 1D / 3D; // Tr
            T[1] = hk; // Tb
            T[2] = hk - 1D / 3D; // Tg

            for (var i = 0; i < 3; i++)
            {
                if (T[i] < 0D)
                    T[i] += 1D;
                if (T[i] > 1D)
                    T[i] -= 1D;

                if (T[i] * 6D < 1D)
                    T[i] = p + (q - p) * 6D * T[i];
                else if (T[i] * 2D < 1)
                    T[i] = q;
                else if (T[i] * 3D < 2)
                    T[i] = p + (q - p) * (2D / 3D - T[i]) * 6D;
                else
                    T[i] = p;
            }

            return FromArgb((float)a, (float)T[0], (float)T[1], (float)T[2]);
        }

        #region Custom Colors Lists

        public static Vector4 BerryColor(Random Rng)
        {
            switch (Rng.Next(0, 6))
            {
                case 0: return FromHtml("#BF4B42");
                case 1: return FromHtml("#FF6380");
                case 2: return FromHtml("#AA3D98");
                case 3: return FromHtml("#FF65F2");
                case 4: return FromHtml("#379B95");
                case 5: return FromHtml("#FFAD5A");
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static Vector4 MushroomHeadColor(Random Rng)
        {
            switch (Rng.Next(0, 6))
            {
                case 0: return FromHtml("#f42e49");
                case 1: return FromHtml("#77608e");
                case 2: return FromHtml("#3474bc");
                case 3: return FromHtml("#9a6c30");
                case 4: return FromHtml("#df665e");
                case 5: return FromHtml("#fcec61");
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static Vector4 MushroomStemColor(Random Rng)
        {
            switch (Rng.Next(0, 3))
            {
                case 0: return FromHtml("#dabdc2");
                case 1: return FromHtml("#bca474");
                case 2: return FromHtml("#ecce92");
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}