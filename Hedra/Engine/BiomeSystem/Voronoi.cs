using System;
using System.Numerics;

namespace Hedra.Engine.BiomeSystem
{
    public class Voronoi
    {
        private const double Sqrt3 = 1.73205080756888;

        private const double Inverse1073741824 = 1.0 / 1073741824.0;

        public Voronoi()
        {
            Frequency = 1.0;
            Displacement = 1.0;
            Seed = 1;
            DistanceEnabled = false;
        }

        public double Frequency { get; set; }

        public double Displacement { get; set; }

        public bool DistanceEnabled { get; set; }

        public int Seed { get; set; }

        public double GetValue(double x, double z)
        {
            x *= Frequency;
            z *= Frequency;
            var num1 = x > 0.0 ? (int)x : (int)x - 1;
            var num3 = z > 0.0 ? (int)z : (int)z - 1;
            double num4 = int.MaxValue;
            var num5 = 0.0;
            var num7 = 0.0;
            for (var z1 = num3 - 2; z1 <= num3 + 2; ++z1)
            for (var x1 = num1 - 2; x1 <= num1 + 2; ++x1)
            {
                var num8 = x1 + ValueNoise(x1, z1, Seed);
                var num10 = z1 + ValueNoise(x1, z1, Seed + 2);
                var num11 = num8 - x;
                var num13 = num10 - z;
                var num14 = num11 * num11 + num13 * num13;
                if (num14 < num4)
                {
                    num4 = num14;
                    num5 = num8;
                    num7 = num10;
                }
            }

            double num15;
            if (DistanceEnabled)
            {
                var num8 = num5 - x;
                var num10 = num7 - z;
                num15 = Math.Sqrt(num8 * num8 + num10 * num10) * Sqrt3 - 1.0;
            }
            else
            {
                num15 = 0.0;
            }

            var x2 = num5 > 0.0 ? (int)num5 : (int)num5 - 1;
            var z2 = num7 > 0.0 ? (int)num7 : (int)num7 - 1;
            return num15 + Displacement * ValueNoise(x2, z2);
        }

        public Vector2 GetGridPoint(double x, double z)
        {
            x *= Frequency;
            z *= Frequency;
            var num1 = x > 0.0 ? (int)x : (int)x - 1;
            var num3 = z > 0.0 ? (int)z : (int)z - 1;
            double num4 = int.MaxValue;
            var num5 = 0.0;
            var num7 = 0.0;
            for (var z1 = num3 - 2; z1 <= num3 + 2; ++z1)
            for (var x1 = num1 - 2; x1 <= num1 + 2; ++x1)
            {
                var num8 = x1 + ValueNoise(x1, z1, Seed);
                var num10 = z1 + ValueNoise(x1, z1, Seed + 2);
                var num11 = num8 - x;
                var num13 = num10 - z;
                var num14 = num11 * num11 + num13 * num13;
                if (num14 < num4)
                {
                    num4 = num14;
                    num5 = num8;
                    num7 = num10;
                }
            }

            return new Vector2((float)num5, (float)num7);
        }

        public int IntValueNoise(int x, int z, int seed)
        {
            var num1 = (1619 * x + 6971 * z + 1013 * seed) & int.MaxValue;
            var num2 = (num1 >> 13) ^ num1;
            return (num2 * (num2 * num2 * 60493 + 19990303) + 1376312589) & int.MaxValue;
        }

        public double ValueNoise(int x, int z)
        {
            return ValueNoise(x, z, 0);
        }

        public double ValueNoise(int x, int z, int seed)
        {
            return 1.0 - IntValueNoise(x, z, seed) * Inverse1073741824;
        }
    }
}