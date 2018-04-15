using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapBuilder
    {
        public int SampleChunk(Vector2 Offset, int SampleLength = 1)
        {
            for (var x = 0; x < Chunk.Width; x++)
            {
                for (var z = 0; z < Chunk.Width; z++)
                {
                    var result = this.Sample(Offset.ToVector3() + new Vector3(x, 0, z));
                    if (result > 0) return result;
                }
            }
            return 0;
        }

        public int Sample(Vector3 Position)
        {
            var biome = World.BiomePool.GetRegion(Position);
            var chunkOffset = World.ToChunkSpace(Position);
            for (var i = 0; i < biome.Structures.Designs.Length; i++)
            {
                var design = biome.Structures.Designs[i];
                var rng = design.BuildRng(chunkOffset);
                if (design.ShouldSetup(chunkOffset, design.BuildTargetPosition(chunkOffset, rng), biome, rng))
                    return i+1;
            }
            return 0;
        }
    }
}
