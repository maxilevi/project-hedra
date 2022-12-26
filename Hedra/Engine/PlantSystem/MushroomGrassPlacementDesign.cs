using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem;

public class MushroomGrassPlacementDesign : PlacementDesign
{
    private readonly MushroomGrassDesign _design = new ();

    public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
    {
        return _design;
    }

    public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
    {
        const float f = 0.005f;
        return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
               World.GetNoise(Position.X * f, Position.Z * f) > 0.25f && UnderChunk.Landscape.RandomGen.Next(0, 5) == 1;
    }
}