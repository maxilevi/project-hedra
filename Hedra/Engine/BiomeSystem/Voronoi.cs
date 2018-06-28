
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Math = System.Math;

namespace Hedra.Engine.BiomeSystem
{
    internal class Voronoi
    {
        private const double Sqrt3 = 1.73205080756888;

        private const double Inverse1073741824 = 1 / 1073741824.0;

        public double Frequency { get; set; }

        public double Displacement { get; set; }

        public bool DistanceEnabled { get; set; }

        public int Seed { get; set; }

        public Voronoi()
        {
            this.Frequency = 1.0;
            this.Displacement = 1.0;
            this.Seed = 1;
            this.DistanceEnabled = false;
        }

        public double GetValue(double x, double z)
        {
            x *= this.Frequency;
            z *= this.Frequency;
            int num1 = x > 0.0 ? (int) x : (int) x - 1;
            int num3 = z > 0.0 ? (int) z : (int) z - 1;
            double num4 = (double) int.MaxValue;
            double num5 = 0.0;
            double num7 = 0.0;
            for (int z1 = num3 - 2; z1 <= num3 + 2; ++z1)
            {
                for (int x1 = num1 - 2; x1 <= num1 + 2; ++x1)
                {
                    double num8 = (double) x1 + this.ValueNoise(x1, z1, this.Seed);
                    double num10 = (double) z1 + this.ValueNoise(x1, z1, this.Seed + 2);
                    double num11 = num8 - x;
                    double num13 = num10 - z;
                    double num14 = num11 * num11 + num13 * num13;
                    if (num14 < num4)
                    {
                        num4 = num14;
                        num5 = num8;
                        num7 = num10;
                    }
                }     
            }
            double num15;
            if (this.DistanceEnabled)
            {
                double num8 = num5 - x;
                double num10 = num7 - z;
                num15 = Math.Sqrt(num8 * num8 + num10 * num10) * Sqrt3 - 1.0;
            }
            else
                num15 = 0.0;
            int x2 = num5 > 0.0 ? (int) num5 : (int) num5 - 1;
            int z2 = num7 > 0.0 ? (int) num7 : (int) num7 - 1;
            return num15 + this.Displacement * this.ValueNoise(x2, z2);
        }

        public int IntValueNoise(int x, int z, int seed)
        {
            int num1 = 1619 * x + 6971 * z + 1013 * seed & int.MaxValue;
            int num2 = num1 >> 13 ^ num1;
            return num2 * (num2 * num2 * 60493 + 19990303) + 1376312589 & int.MaxValue;
        }

        public double ValueNoise(int x, int z)
        {
            return this.ValueNoise(x, z, 0);
        }

        public double ValueNoise(int x, int z, int seed)
        {
            return 1.0 - this.IntValueNoise(x, z, seed) * Inverse1073741824;
        }
    }
}