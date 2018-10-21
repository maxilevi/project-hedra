using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeGenerationDesign :  BiomeGenerationDesign
    {
        public override bool HasRivers { get; set; } = true;
        public override bool HasPaths { get; set; } = true;
        public override bool HasDirt { get; set; } = true;

        public override float GetDensity(float X, float Y, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            var density = 0.0;
            if(World.MenuSeed != World.Seed)
            {
                if (HeightCache.ContainsKey(new Vector2(X, Z)))
                {
                    var mult = HeightCache[new Vector2(X, Z)][0];
                    if (mult > 0.0)
                    {
                       /*density -= Mathf.Clamp( OpenSimplexNoise.Evaluate(X * 0.008, Y * 0.008, Z * 0.008) * 16.0,
                            0, float.MaxValue
                        );*/
                    }
                }
            }
            return (float)density;
        }

        public override bool HasHeightSubtype(float X, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            return HeightCache.ContainsKey(new Vector2(X, Z));
        }
        
        public override BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            /*
            double height = HeightCache[new Vector2(X, Z)][1];
            double realHeight = (CurrentHeight - height) / HeightCache[new Vector2(X, Z)][2];

            if (Math.Abs(realHeight - 32.0) < 0.5f)
            {
                if (Y > 28.0)
                    return BlockType.Grass;
            }*/
            return Type;
        }

        public override float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache, out BlockType Blocktype)
        {
            var height = 0.0;
            Blocktype = BlockType.Air;
            
            AddBaseHeight(X, Z, ref height, ref Blocktype, out var baseHeight);
            AddMountainHeight(X, Z, ref height, ref Blocktype);
            AddMountHeight(X, Z, ref height, ref Blocktype, HeightCache);
            AddBigMountainsHeight(X, Z,ref height, ref Blocktype, HeightCache);
            AddLakes(X, Z, ref height);
            AddStones(X, Z, ref height, ref Blocktype, HeightCache);
            
            return (float)height + BiomeGenerator.SmallFrequency(X, Z) * 1.5f;
        }

        private static void AddLakes(float X, float Z, ref double Height)
        {
            var lakeNoise = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.001, Z * 0.001));
            var frequency = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.0005, Z * 0.0005));
            Height += frequency * -lakeNoise * 48.0;
        }

        private static void AddStones(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            var stones = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.005, Z * 0.005) - .5) *
                         48.0; // * Math.Min(moutainHeight, 1);
            Height += stones;
            if (stones > 0)
            {
                HeightCache?.Add(new Vector2(X, Z), new[] { (float)stones });
                Height += BiomeGenerator.SmallFrequency(X + 234, Z + 12123) * 2.0;
                Type = BlockType.Stone;
            }
        }

        private static void AddBigMountainsHeight(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            if (World.Seed == World.MenuSeed) return;

            var rawMountainHeight = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.00075, Z * 0.00075) - .15);
            var moutainHeight = rawMountainHeight * 200.0;
            if (moutainHeight > 0)
            {
                /*
                var stones = Math.Min(Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.0025, Z * 0.0025) - .5) * 2048.0, 16.0)
                * Math.Min(moutainHeight, 1);
                Height += stones;
                if (stones > 0)
                {
                    HeightCache?.Add(new Vector2(X, Z), new []{ (float) stones});
                    Height += BiomeGenerator.SmallFrequency(X + 234, Z + 12123) * 2.0;
                    Type = BlockType.Stone;
                }*/
            }
            Height += moutainHeight;
        }

        private static void AddMountHeight(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            /*var mountHeight = Mathf.Clamp((OpenSimplexNoise.Evaluate(X * 0.004, Z * 0.004) - .6f) * 2048, 0.0, 32.0);

            if (mountHeight > 0)
            {
                Type = BlockType.Stone;
                var mod = (World.MenuSeed == World.Seed) ? 0 : .4f;//Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.005f, z * 0.005f) * .5f - .1f, 0.0, 2.0);
                mountHeight *= mod;
                var Mult = 1;//Mathf.Clamp(OpenSimplexNoise.Evaluate(x * 0.005, z * 0.005) * 4.0, 0, 1);
                mountHeight *= Mult;

                if (mountHeight > 0)
                    HeightCache?.Add(new Vector2(X, Z), new[] { (float)mountHeight, (float)Height, (float)(Mult * mod) });
                Height += mountHeight;
            }

            if (mountHeight <= 1.0)
                Type = BlockType.Grass;*/
        }
        
        private static void AddMountainHeight(float X, float Z, ref double Height, ref BlockType Type)
        {
            var grassMountHeight = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.0008, Z * 0.0008) * 80.0);
            if (grassMountHeight != 0)
            {
                Type = BlockType.Grass;
            }
            Height += grassMountHeight;
        }

        private static void AddBaseHeight(float X, float Z, ref double Height, ref BlockType Type, out double BaseHeight)
        {
            var baseHeight = /*OpenSimplexNoise.Evaluate(X * 0.00005, Z * 0.00005) * 48.0 +*/ BiomePool.SeaLevel;
            var grassHeight = (OpenSimplexNoise.Evaluate(X * 0.004, Z * 0.004) + .25) * 3.0;
            Type = BlockType.Grass;
            Height += baseHeight + grassHeight;
            BaseHeight = baseHeight;
        }
    }
}
