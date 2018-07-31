using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public static class Colors
    {
        public static Vector4 DarkRed { get; } = new Vector4(.5f, 0, 0, 1);
        public static Vector4 Red { get; } = new Vector4(1,0,0,1);
        public static Vector4 Sienna { get; } = new Vector4(0.625f, 0.3203125f, 0.17578125f, 1);
        public static Vector4 Gray { get; } = new Vector4(.5f,.5f,.5f,1f);
        public static Vector4 DeepSkyBlue { get; } = new Vector4(0, 0.74609375f, 1, 1);
        public static Vector4 Transparent { get; } = new Vector4(0,0,0,0);
        public static Vector4 Blue { get; } = new Vector4(0,0,1,1);

        public static Vector4 FromHtml(string Hex)
        {
            int argb = int.Parse(Hex.Replace("#", string.Empty), NumberStyles.HexNumber);

            return Colors.FromArgb((byte)((argb & -16777216) >> 0x18),
            (byte)((argb & 0xff0000) >> 0x10),
            (byte)((argb & 0xff00) >> 8),
            (byte)(argb & 0xff));
        }

        public static Vector4 FromArgb(float A, float R, float G, float B)
        {
            return new Vector4(R,G,B,A);
        }

        public static Vector4 FromArgb(byte A, byte R, byte G, byte B)
        {
            var inverse = 1 / 256f;
            return new Vector4(R * inverse, G * inverse, B * inverse, A * inverse);
        }

        #region Custom Colors Lists
        public static Vector4 BerryColor(Random Rng)
        {
            switch (Rng.Next(0, 6))
            {
                case 0: return Colors.FromHtml("#BF4B42");
                case 1: return Colors.FromHtml("#FF6380");
                case 2: return Colors.FromHtml("#AA3D98");
                case 3: return Colors.FromHtml("#FF65F2");
                case 4: return Colors.FromHtml("#379B95");
                case 5: return Colors.FromHtml("#FFAD5A");
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public static Vector4 MushroomHeadColor(Random Rng)
        {
            switch (Rng.Next(0, 6))
            {
                case 0: return Colors.FromHtml("#f42e49");
                case 1: return Colors.FromHtml("#77608e");
                case 2: return Colors.FromHtml("#3474bc");
                case 3: return Colors.FromHtml("#9a6c30");
                case 4: return Colors.FromHtml("#df665e");
                case 5: return Colors.FromHtml("#fcec61");
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public static Vector4 MushroomStemColor(Random Rng)
        {
            switch (Rng.Next(0, 3))
            {
                case 0: return Colors.FromHtml("#dabdc2");
                case 1: return Colors.FromHtml("#bca474");
                case 2: return Colors.FromHtml("#ecce92");
                default: throw new ArgumentOutOfRangeException();
            }
        }
        #endregion
    }
}
