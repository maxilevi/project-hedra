using System;
using OpenTK;

namespace Hedra.Core
{
    public static class Unique
    {
        public static int GenerateSeed(Vector2 Offset)
        {
            return seed2(seed2((int) Offset.X * 1947) ^ seed2((int) Offset.Y * 2904));
        }

        private static int seed2(int _s)
        {
            var s = 192837463 ^ System.Math.Abs(_s);
            var a = 1664525;
            var c = 1013904223;
            var m = 4294967296;
            return (int)((s * a + c) % m);
        }

        private static int GetSeedXY(int x, int y)
        {
            int sx = seed2(x * 1947);
            int sy = seed2(y * 2904);
            return seed2(sx ^ sy);
        }

        public static int RandomSeed() => RandomSeed(Utils.Rng);
        
        public static int RandomSeed(Random Rng) => Utils.Rng.Next(int.MinValue, int.MaxValue);
    }
}