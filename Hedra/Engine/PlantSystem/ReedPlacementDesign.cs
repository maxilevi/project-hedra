using System;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class ReedPlacementDesign : PlacementDesign
    {        
        private readonly PlantDesign _reedDesign;

        public ReedPlacementDesign()
        {
            _reedDesign = new ReedDesign();
        }
        
        public override bool CanBeHidden => true;
              
        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return _reedDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            float diff;
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Seafloor
                   && (diff = BiomePool.SeaLevel - World.GetHighestY((int) Position.X, (int) Position.Z)) < 2.5
                   && diff > .5
                   && OpenSimplexNoise.Evaluate(Position.X * 0.004f, Position.Z * 0.004f) > .2f
                   && Vector3.Dot(Physics.NormalAtPosition(Position), Vector3.UnitY) > .75
                   && UnderChunk.Landscape.RandomGen.Next(0, 5) == 1;
        }
    }
}