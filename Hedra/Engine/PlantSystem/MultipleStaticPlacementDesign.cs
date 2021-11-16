using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem
{
    public abstract class MultipleStaticPlacementDesign : PlacementDesign
    {
        protected abstract PlantDesign[] Designs { get; }

        protected abstract BlockType[] Types { get; }

        protected abstract PlantDesign SelectDesign(Random Rng);

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return SelectDesign(Rng);
        }

        protected abstract bool ShouldPlace(Random Rng);

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return Array.IndexOf(Types, World.GetHighestBlockAt(Position.X, Position.Z).Type) != -1 &&
                   ShouldPlace(UnderChunk.Landscape.RandomGen) && World.MenuSeed != World.Seed;
        }
    }
}