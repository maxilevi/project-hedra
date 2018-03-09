
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class WeedsPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _grassDesign;
        private readonly PlantDesign _wheatDesign;

        public WeedsPlacementDesign()
        {
            _grassDesign = new GrassDesign();
            _wheatDesign = new WheatDesign();
        }

        public override bool CanBeHidden => true;

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return UnderChunk.Landscape.RandomGen.Next(0, 7) != 1 ? _grassDesign : _wheatDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                SimplexNoise.Noise.Generate(Position.X * 0.025f, Position.Z * 0.025f) > 0.55f;
        }
    }
}
