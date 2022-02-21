/*
 * Author: Zaphyk
 * Date: 15/02/2016
 * Time: 09:19 p.m.
 *
 */

using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.BiomeSystem
{
    /// <summary>
    ///     Description of BiomeGenerator.
    /// </summary>
    public abstract class BiomeGenerator : IDisposable
    {
        protected readonly FastNoiseSIMD Noise;

        protected BiomeGenerator(Chunk Parent)
        {
            RandomGen = GenerateRng(new Vector2(Parent.OffsetX, Parent.OffsetZ));
            OffsetX = Parent.OffsetX;
            OffsetZ = Parent.OffsetZ;
            this.Parent = Parent;
            Seed = World.Seed;
            Noise = new FastNoiseSIMD(Seed);
        }

        protected int OffsetX { get; }
        protected int OffsetZ { get; }
        protected Chunk Parent { get; }
        protected int Seed { get; }
        public Random RandomGen { get; set; }
        public bool BlocksSetted { get; protected set; }
        public bool BlocksDefined { get; protected set; }
        public bool StructuresPlaced { get; protected set; }
        public bool FullyGenerated => StructuresPlaced && BlocksDefined;

        public void Dispose()
        {
            Noise.Dispose();
        }

        protected abstract void PlaceEnvironment(RegionCache Cache);

        protected abstract void DoTreeAndStructurePlacements(RegionCache Cache);

        protected abstract ChunkDetails DefineBlocks(Block[] Blocks);

        public ChunkDetails GenerateBlocks(Block[] Blocks)
        {
            if (BlocksDefined) throw new ArgumentException("Cannot generate a chunk multiple times");
            CheckForNearbyLandforms();
            CheckForNearbyStructures();
            BlocksSetted = true;
            var details = DefineBlocks(Blocks);
            BlocksDefined = true;
            return details;
        }

        public void GenerateEnvironment(RegionCache Cache)
        {
            if (StructuresPlaced) throw new ArgumentException("Cannot generate a chunk multiple times");
            DoTreeAndStructurePlacements(Cache);
            PlaceEnvironment(Cache);
            StructuresPlaced = true;
        }

        private void CheckForNearbyStructures()
        {
            World.StructureHandler.CheckStructures(new Vector2(Parent.OffsetX, Parent.OffsetZ));
        }

        private void CheckForNearbyLandforms()
        {
            World.StructureHandler.CheckLandforms(new Vector2(Parent.OffsetX, Parent.OffsetZ));
        }

        public static float PathFormula(float X, float Z)
        {
            return (float)Math.Max(0, 0.5 - Math.Abs(World.GetNoise(X * 0.0009f, Z * 0.0009f) - 0.2) - 0.425f) * 1f;
        }

        public static float SmallFrequency(float X, float Z)
        {
            return World.GetNoise(X * 0.2f, Z * 0.2f) * -0.15f * World.GetNoise(X * 0.035f, Z * 0.035f) * 2.0f;
        }

        public static Random GenerateRng(Vector2 Offset)
        {
            return new Random(Unique.GenerateSeed(Offset));
        }
    }
}