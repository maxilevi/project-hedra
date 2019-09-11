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
        private static FastNoise _noise = new FastNoise(214121);
        public override float GetDensity(float X, float Y, float Z, ref BlockType Type)
        {
            var bigMountainHeight = BigMountainsHeight(X, Y);
            var density = 0f;
            
            var lakes = _noise.GetSimplexFractalWithFrequency(X, Y, Z, 0.005f) * 0.25f * Chunk.BlockSize;
            var overhangs = 1//_noise.GetSimplexFractalWithFrequency(X, Y, Z, 0.0005f).Clamp01()
                            * _noise.GetSimplexFractalWithFrequency(X, Y, Z, 0.00075f).Clamp01()
                            * 48.0f 
                            * Chunk.BlockSize;
   
            if (Math.Abs(overhangs) > 0)
            {
                Type = BlockType.Stone;
            }


            density += overhangs;
            density += lakes;
            density += (World.GetNoise(X * 0.2f, Y * 0.2f, Z * 0.2f) * -0.15f * World.GetNoise(X * 0.035f, Y * 0.2f, Z * 0.035f) * 2.0f) * 7.5f;
            /*
            _noise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);
            var cell = _noise.GetCellular(X, Y, Z);
            cell *= 24.0f * bigMountainHeight * Chunk.BlockSize;
            density += cell;*/
            /*_noise.SetFrequency(0.00075f);
            density += _noise.GetSimplex(X, Y, Z) * -24.0f * bigMountainHeight * Chunk.BlockSize;
            _noise.SetFrequency(0.0015f);
            var overhangs = _noise.GetSimplex(X, Y, Z) * 48.0f * bigMountainHeight  * Chunk.BlockSize;
            density += overhangs;
            if (Math.Abs(overhangs) > 0)
            {
                Type = BlockType.Stone;
            }*/
            return density;
        }

        public override bool HasHeightSubtype(float X, float Z, Dictionary<Vector2, float[]> HeightCache)
        {
            return HeightCache.ContainsKey(new Vector2(X, Z));
        }
        
        public override BlockType GetHeightSubtype(float X, float Y, float Z, float CurrentHeight, BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            return Type;
        }

        public override float GetHeight(float X, float Z, Dictionary<Vector2, float[]> HeightCache, out BlockType Blocktype)
        {
            var height = 0.0;
            Blocktype = BlockType.Air;

            AddBaseHeight(X, Z, ref height, ref Blocktype, out var baseHeight);
            //AddMountainHeight(X, Z, ref height, ref Blocktype);
            //AddBigMountainsHeight(X, Z,ref height, ref Blocktype, HeightCache);
            //AddLakes(X, Z, ref height);
            //AddStones(X, Z, ref height, ref Blocktype, HeightCache);
            /* This one should be last */
            //AddMountHeight(X, Z, ref height, ref Blocktype, HeightCache);

            const float buffer = 28;
            var maxHeight = Chunk.Height - Chunk.BlockSize;
            var cutoff = maxHeight - buffer;
            var lerp = Mathf.Clamp((float) (height - cutoff) / buffer, 0f, 1f);
            height = Mathf.Lerp((float)height, maxHeight, lerp);

            return (float) height;// + BiomeGenerator.SmallFrequency(X, Z) * 1.5f;
        }

        private static void AddLakes(float X, float Z, ref double Height)
        {
            var lakeNoise = Math.Pow(Math.Max(0, World.GetNoise(X * 0.001f, Z * 0.001f)), 2);
            var frequency = Math.Max(0, World.GetNoise(X * 0.0005f, Z * 0.0005f));
            Height += frequency * -lakeNoise * 96.0;
        }

        protected static void AddStones(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            var stones = Math.Max(0, World.GetNoise(X * 0.005f, Z * 0.005f) - .5) *
                         48.0; // * Math.Min(moutainHeight, 1);
            Height += stones;
            if (stones > 0)
            {
                HeightCache?.Add(new Vector2(X, Z), new[] { (float)stones });
                Height += BiomeGenerator.SmallFrequency(X + 234, Z + 12123) * 2.0;
                Type = BlockType.Stone;
            }
        }

        private static float BigMountainsHeight(float X, float Z)
        {
            _noise.SetFrequency(0.0004f);
            return _noise.GetSimplex(X, Z).Clamp01();
        }

        private static void AddBigMountainsHeight(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            var rawMountainHeight = BigMountainsHeight(X, Z);
            var moutainHeight = rawMountainHeight * 64.0;
            if (moutainHeight > 0)
            {
               // Type = BlockType.Stone;
                //var smallerMountains = World.GetNoise(X * 0.02f, Z * 0.02f) * 12f * World.GetNoise(X * 0.001f, Z * 0.001f);
                //Height += smallerMountains * Math.Min(1f, rawMountainHeight);
            }
            Height += moutainHeight;
        }

        private static void AddMountHeight(float X, float Z, ref double Height, ref BlockType Type, Dictionary<Vector2, float[]> HeightCache)
        {
            var max = 12f;
            var grassModifier = 2f;
            var mu = Math.Min(Math.Max((World.GetNoise(X * 0.004f, Z * 0.004f) - .6f) * 96f, 0.0), max) / max;
            var mountHeight = mu * max * (Height > 0 ? 1 : 0);
            if (mountHeight > 0)
            {
                Type = BlockType.Stone;
                var mult = Math.Min(Math.Max(World.GetNoise(X * 0.0005f, Z * 0.0005f) * 4.0f, 0.0), 1.0);
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
            var rawMountainHeight = Math.Pow(Math.Min(Math.Max(0f, World.GetNoise(X * 0.0005f, Z * 0.0005f)), 1), 3);
            Height += rawMountainHeight * 512.0;
        }

        protected static void AddBaseHeight(float X, float Z, ref double Height, ref BlockType Type, out double BaseHeight)
        {
            var baseHeight = /*World.GetNoise(X * 0.00005f, Z * 0.00005f) * 48.0f +*/ BiomePool.SeaLevel;
            var grassHeight = (_noise.GetSimplexWithFrequency(X, Z, 0.0001f)) * 16.0f;
            Type = BlockType.Grass;
            Height += baseHeight + grassHeight;
            BaseHeight = baseHeight;
        }
    }
}
