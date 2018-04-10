using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.Player.MapSystem
{
    public class MapBuilder
    {
        public int[][] Build(Vector2 Size, Vector2 Offset, int StepSize)
        {
            var grid = new int[(int) Size.X][]; 
            for (var x = 0; x < Size.X; x++)
            {
                grid[x] = new int[ (int) Size.Y];
                for (var z = 0; z < Size.Y; z++)
                {
                    grid[x][z] = this.Sample(new Vector3(Offset.X + x * StepSize, 0, Offset.Y + z * StepSize));
                }
            }
            return grid;
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
