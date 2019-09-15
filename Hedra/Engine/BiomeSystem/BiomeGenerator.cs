/*
 * Author: Zaphyk
 * Date: 15/02/2016
 * Time: 09:19 p.m.
 *
 */
using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Core;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.StructureSystem;

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
        private bool _firstGeneration = true;
        private bool _environmentPlaced;
        protected readonly FastNoiseSIMD Noise;

        protected BiomeGenerator(Chunk RefChunk)
        {
            this.RandomGen = BiomeGenerator.GenerateRng(new Vector2(RefChunk.OffsetX, RefChunk.OffsetZ));
            this.OffsetX = RefChunk.OffsetX;
            this.OffsetZ = RefChunk.OffsetZ;
            this.Chunk = RefChunk;
            this.Seed = World.Seed;
            this.Noise = new FastNoiseSIMD(Seed);
        }

        protected abstract void PlaceEnvironment(RegionCache Cache, Predicate<PlacementDesign> Filter);

        protected abstract void DoTreeAndStructurePlacements(Block[][][] Blocks, RegionCache Cache, int Lod);
        
        protected abstract void DefineBlocks(Block[][][] Blocks, RegionCache Cache, int Lod, Func<int, int, bool> Filter);
        
        public void Generate(Block[][][] Blocks, RegionCache Cache)
        {
            if (FullyGenerated) throw new ArgumentException($"Cannot generate a chunk multiple times");
            if (_firstGeneration)
            {
                this.CheckForNearbyStructures();
                this.BuildArray(Blocks);
                this.BlocksSetted = true;
            }

            var lod = 1;
            this.DefineBlocks(Blocks, Cache, 1, (X,Z) => true);
            GeneratedLod = lod;
            if (_firstGeneration)
            {
                this.DoTreeAndStructurePlacements(Blocks, Cache, lod);
                this.PlaceEnvironment(Cache, D => D.CanBePlacedInPartialGeneration);
            }
            if (lod == 1 && !_environmentPlaced)
            {
                this.PlaceEnvironment(Cache, D => !D.CanBePlacedInPartialGeneration);
                _environmentPlaced = true;
            }
            this.StructuresPlaced = true;
            this._firstGeneration = false;
            this.FullyGenerated = lod == 1;
        }

        private void CheckForNearbyStructures()
        {
            //StructureHandler.CheckStructures( new Vector2(Chunk.OffsetX, Chunk.OffsetZ) );
        }
        
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
        
        public static float PathFormula(float X, float Z)
        {
            return (float) Math.Max(0, (0.5 - Math.Abs(World.GetNoise(X * 0.0009f, Z *  0.0009f) - 0.2)) - 0.425f) * 1f;
        }
        
        public static float SmallFrequency(float X, float Z)
        {
            return (float) (World.GetNoise(X * 0.2f, Z * 0.2f) * -0.15f * World.GetNoise(X * 0.035f, Z * 0.035f) * 2.0f);
        }
        
        public static Random GenerateRng(Vector2 Offset)
        {
            return new Random( Unique.GenerateSeed(Offset) );
        }
        
        public void Dispose()
        {
            Noise.Dispose();
        }
    }
}
 