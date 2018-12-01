/*
 * Author: Zaphyk
 * Date: 15/02/2016
 * Time: 09:19 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.ComplexMath;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.BiomeSystem
{
    /// <summary>
    /// Description of BiomeGenerator.
    /// </summary>
    public abstract class BiomeGenerator : IDisposable
    {
        protected int OffsetX { get; }
        protected int OffsetZ { get; }
        protected Chunk Chunk { get; }
        protected int Seed { get; }
        public Random RandomGen { get; set; }
        public bool BlocksSetted { get; protected set; }
        public bool StructuresPlaced { get; protected set; }
        public bool FullyGenerated { get; protected set; }
        public int GeneratedLod { get; set; } = -1;
        public bool HasToGenerateMoreData => Chunk.Lod < GeneratedLod && !FullyGenerated;

        protected BiomeGenerator(Chunk RefChunk)
        {
            this.RandomGen = BiomeGenerator.GenerateRng(new Vector2(RefChunk.OffsetX, RefChunk.OffsetZ));
            this.OffsetX = RefChunk.OffsetX;
            this.OffsetZ = RefChunk.OffsetZ;
            this.Chunk = RefChunk;
            this.Seed = World.Seed;
        }

        public abstract void Generate(Block[][][] Blocks, RegionCache Cache);
        
        public abstract void DefineBlocks(Block[][][] Blocks, RegionCache Cache, int Lod, Func<int, int, bool> Filter);
        
        public void BuildArray(Block[][][] Blocks)
        {
            for(var x = 0; x < (int) (Chunk.Width / Chunk.BlockSize); x++)
            {
                Blocks[x] = new Block[Chunk.Height][];
                for(var y = 0; y <  Chunk.Height; y++)
                {
                    Blocks[x][y] = new Block[(int) (Chunk.Width / Chunk.BlockSize)];
                }
            }
            this.Chunk.CalculateBounds();
        }

        public void Cull(Block[][][] Blocks, ChunkSparsity Sparsity)
        {
            for (var x = 0; x < Blocks.Length; x++)
            {
                for (var y = 0; y < Blocks.Length; y++)
                {
                    if (y < Sparsity.MinimumHeight || y > Sparsity.MaximumHeight)
                        Blocks[x][y] = null;
                }
            }
        }
        
        public static float PathFormula(float X, float Z)
        {
            return (float) Math.Max(0, (0.5 - Math.Abs(OpenSimplexNoise.Evaluate(X * 0.0009f, Z *  0.0009f) - 0.2)) - 0.425f) * 1f;
        }
        
        public static float SmallFrequency(float X, float Z)
        {
            return (float) (OpenSimplexNoise.Evaluate(X * 0.2, Z * 0.2) * -0.15f * OpenSimplexNoise.Evaluate(X * 0.035, Z * 0.035) * 2.0f);
        }

        static int seed2(int _s)
        {
            var s = 192837463 ^ System.Math.Abs(_s);
            var a = 1664525;
            var c = 1013904223;
            var m = 4294967296;
            return (int)((s * a + c) % m);
        }

        static int GetSeedXY(int x, int y)
        {
            int sx = seed2(x * 1947);
            int sy = seed2(y * 2904);
            return seed2(sx ^ sy);
        }

        public static int GenerateSeed(Vector2 Offset)
        {
            return seed2(seed2((int) Offset.X * 1947) ^ seed2((int) Offset.Y * 2904));
        }
        
        public static Random GenerateRng(Vector2 Offset)
        {
            return new Random( GenerateSeed(Offset) );
        }
        
        public void Dispose()
        {

        }
    }
}
 