using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public abstract class CollectiblePlacementDesign : PlacementDesign
    {
        protected abstract PlantDesign Design { get; }
        
        protected abstract BlockType[] Types { get; }
        
        protected abstract bool ShouldPlace(Random Rng);

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return Design;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return Array.IndexOf(Types, World.GetHighestBlockAt(Position.X, Position.Z).Type) != -1 &&
                   ShouldPlace(UnderChunk.Landscape.RandomGen) && World.MenuSeed != World.Seed;
        }
    }
}