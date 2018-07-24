using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    internal class MushroomPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _mushroomDesign;

        public MushroomPlacementDesign()
        {
            _mushroomDesign = new MushroomDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return _mushroomDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                   UnderChunk.Landscape.RandomGen.Next(0, 2000) == 1 && World.MenuSeed != World.Seed;
        }
    }
}