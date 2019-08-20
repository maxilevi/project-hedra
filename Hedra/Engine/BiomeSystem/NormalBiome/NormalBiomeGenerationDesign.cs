using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Core;
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
            AddBigMountainsHeight(X, Z,ref height, ref Blocktype, HeightCache);
            AddLakes(X, Z, ref height);
            AddStones(X, Z, ref height, ref Blocktype, HeightCache);
            /* This one should be last */
            AddMountHeight(X, Z, ref height, ref Blocktype, HeightCache);

            const float buffer = 28;
            var maxHeight = Chunk.Height - Chunk.BlockSize;
            var cutoff = maxHeight - buffer;
            var lerp = Mathf.Clamp((float) (height - cutoff) / buffer, 0f, 1f);
            height = Mathf.Lerp((float)height, maxHeight, lerp);

            return (float)height + BiomeGenerator.SmallFrequency(X, Z) * 1.5f;
        }

        private static void AddLakes(float X, float Z, ref double Height)
        {
            var lakeNoise = Math.Pow(Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.001, Z * 0.001)), 2);
            var frequency = Math.Max(0, OpenSimplexNoise.Evaluate(X * 0.0005, Z * 0.0005));
            Height += frequency * -lakeNoise * 96.0;
        }

        protected static void AddStones(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
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
            var rawMountainHeight = Math.Pow(Math.Min(Math.Max(-1f, OpenSimplexNoise.Evaluate(X * 0.000075, Z * 0.000075) + .25f), 1), 1);
            var moutainHeight = rawMountainHeight * 384.0;
            if (moutainHeight > 0)
            {
                var smallerMountains = OpenSimplexNoise.Evaluate(X * 0.02, Z * 0.02) * 12f * OpenSimplexNoise.Evaluate(X * 0.001, Z * 0.001);
                //Height += smallerMountains * Math.Min(1f, rawMountainHeight);
            }
            Height += moutainHeight;
        }

        private static void AddMountHeight(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            var max = 12f;
            var grassModifier = 2f;
            var mu = Math.Min(Math.Max((OpenSimplexNoise.Evaluate(X * 0.004, Z * 0.004) - .6f) * 96, 0.0), max) / max;
            var mountHeight = mu * max * (Height > 0 ? 1 : 0);
            if (mountHeight > 0)
            {
                Type = BlockType.Stone;
                var mult = Math.Min(Math.Max(OpenSimplexNoise.Evaluate(X * 0.0005, Z * 0.0005) * 4.0, 0.0), 1.0);
                mountHeight *= mult;
                if (HeightCache != null)
                {
                    var position = new Vector2(X, Z);
                    if(HeightCache.ContainsKey(position))
                        HeightCache[position][0] += (float)mountHeight;
                    else
                        HeightCache.Add(position, new[] { (float)mountHeight, (float)(Height + max - grassModifier)});
                }
                Height += mountHeight;
            }
        }
        
        private static void AddMountainHeight(float X, float Z, ref double Height, ref BlockType Type)
        {
            var rawMountainHeight = Math.Pow(Math.Min(Math.Max(0f, OpenSimplexNoise.Evaluate(X * 0.0005, Z * 0.0005)), 1), 3);
            Height += rawMountainHeight * 512.0;
        }

        protected static void AddBaseHeight(float X, float Z, ref double Height, ref BlockType Type, out double BaseHeight)
        {
            var baseHeight = /*OpenSimplexNoise.Evaluate(X * 0.00005, Z * 0.00005) * 48.0 +*/ BiomePool.SeaLevel;
            var grassHeight = (OpenSimplexNoise.Evaluate(X * 0.004, Z * 0.004) + .25) * 3.0;
            Type = BlockType.Grass;
            Height += baseHeight + grassHeight;
            BaseHeight = baseHeight;
        }
    }
}
