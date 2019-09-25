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
        protected Chunk Parent { get; }
        protected int Seed { get; }
        public Random RandomGen { get; set; }
        public bool BlocksSetted { get; protected set; }
        public bool BlocksDefined { get; protected set; }
        public bool StructuresPlaced { get; protected set; }
        public bool FullyGenerated => StructuresPlaced && BlocksDefined;
        protected readonly FastNoiseSIMD Noise;

        protected BiomeGenerator(Chunk RefParent)
        {
            this.RandomGen = BiomeGenerator.GenerateRng(new Vector2(RefParent.OffsetX, RefParent.OffsetZ));
            this.OffsetX = RefParent.OffsetX;
            this.OffsetZ = RefParent.OffsetZ;
            this.Parent = RefParent;
            this.Seed = World.Seed;
            this.Noise = new FastNoiseSIMD(Seed);
        }

        protected abstract void PlaceEnvironment(RegionCache Cache);

        protected abstract void DoTreeAndStructurePlacements(RegionCache Cache);
        
        protected abstract ChunkDetails DefineBlocks(Block[] Blocks);
        
        public ChunkDetails GenerateBlocks(Block[] Blocks)
        {
            if (BlocksDefined) throw new ArgumentException($"Cannot generate a chunk multiple times");
            CheckForNearbyStructures();
            BlocksSetted = true;
            var details = DefineBlocks(Blocks);
            BlocksDefined = true;
            return details;
        }

        public void GenerateEnvironment(RegionCache Cache)
        {
            if (StructuresPlaced) throw new ArgumentException($"Cannot generate a chunk multiple times");
            this.DoTreeAndStructurePlacements(Cache);
            this.PlaceEnvironment(Cache);
            StructuresPlaced = true;
        }

        private void CheckForNearbyStructures()
        {
            //StructureHandler.CheckStructures( new Vector2(Chunk.OffsetX, Chunk.OffsetZ) );
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
 