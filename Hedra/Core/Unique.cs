using System;
using System.Numerics;

namespace Hedra.Core
{
    public static class Unique
    {
        public static int GenerateSeed(Vector2 Offset)
        {
            return seed2(seed2((int)Offset.X * 1947) ^ seed2((int)Offset.Y * 2904));
        }

        private static int seed2(int _s)
        {
            var s = 192837463 ^ Math.Abs(_s);
            var a = 1664525;
            var c = 1013904223;
            var m = 4294967296;
            return (int)((s * a + c) % m);
        }

        private static int GetSeedXY(int x, int y)
        {
            var sx = seed2(x * 1947);
            var sy = seed2(y * 2904);
            return seed2(sx ^ sy);
        }

        public static int RandomSeed()
        {
            return RandomSeed(Utils.Rng);
        }

        public static int RandomSeed(Random Rng)
        {
            return Utils.Rng.Next(int.MinValue, int.MaxValue);
        }
        
        public static int GetSeed(Vector2 Position)
        {
            unchecked
            {
                var seed = 17;
                seed = seed * 31 + Position.X.GetHashCode();
                seed = seed * 31 + Position.Y.GetHashCode();
                seed = seed * 31 + World.Seed.GetHashCode();
                return seed;
            }
        }
    }
}