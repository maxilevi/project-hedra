using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class RockPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _rockDesign;

        public RockPlacementDesign()
        {
            _rockDesign = new RockDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return _rockDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            var block = World.GetHighestBlockAt(Position.X, Position.Z);
            return block.Type != BlockType.Water && block.Type != BlockType.Seafloor && (World.Seed != World.MenuSeed ?
                UnderChunk.Landscape.RandomGen.Next(0, 20000) == 1 : UnderChunk.Landscape.RandomGen.Next(0, 950) == 1);
        }
    }
}
