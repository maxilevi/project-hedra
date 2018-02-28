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
        public static Vector4 Red { get; } = new Vector4(1,0,0,1);
        public static Vector4 Sienna { get; } = new Vector4(0.625f, 0.3203125f, 0.17578125f, 1);
        public static Vector4 Gray { get; } = new Vector4(.5f,.5f,.5f,1f);
        public static Vector4 DeepSkyBlue { get; } = new Vector4(0, 0.74609375f, 1, 1);
        public static Vector4 Transparent { get; } = new Vector4(0,0,0,0);
        public static Vector4 Blue { get; } = new Vector4(0,0,1,1);

        public static Vector4 FromHtml(string Hex)
        {
            int argb = int.Parse(Hex.Replace("#", ""), NumberStyles.HexNumber);

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
    }
}
