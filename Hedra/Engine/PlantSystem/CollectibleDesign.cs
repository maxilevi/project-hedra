using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public abstract class CollectibleDesign : PlacementDesign
    {
        protected abstract PlantDesign Design { get; }
        
        protected abstract BlockType Type { get; }
        
        protected abstract bool ShouldPlace(Random Rng);

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return Design;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == Type &&
                   ShouldPlace(UnderChunk.Landscape.RandomGen) && World.MenuSeed != World.Seed;
        }
    }
}