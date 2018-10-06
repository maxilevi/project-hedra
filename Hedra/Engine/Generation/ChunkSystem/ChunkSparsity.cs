using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkSparsity
    {
        public float MiniumHeight { get; set; }
        public float MaximumHeight { get; set; }

        private ChunkSparsity() { }

        private static Vector2 CalculateSparsity(Chunk Parent)
        {
            var lowest = (float)Parent.BoundsY;
            var highest = 0f;
            for (var x = 0; x < Parent.BoundsX; x++)
            {
                for (var z = 0; z < Parent.BoundsZ; z++)
                {
                    var l = Lowest(x, z, Parent);
                    var h = Highest(x, z, Parent);
                    if (l < lowest) lowest = l;
                    if (h > highest) highest = h;
                } 
            }
            return new Vector2(lowest-2, highest+2);
        }

        private static float Highest(int X, int Z, Chunk Parent)
        {
            for (var y = Parent.BoundsY-1; y > -1; y--)
            {
                var type = Parent[X][y][Z].Type;
                if (type != BlockType.Air)
                    return y+1;
            }
            return 0;
        }

        private static float Lowest(int X, int Z, Chunk Parent)
        {
            for (var y = 0; y < Parent.BoundsY; y++)
            {
                var type = Parent[X][y][Z].Type;
                if (type == BlockType.Air || type == BlockType.Water)
                    return y-1;
            }
            return 0;
        }
        
        public static ChunkSparsity From(Chunk Parent)
        {
            var sparsity = CalculateSparsity(Parent);
            var percentage = ((sparsity.Y - sparsity.X) / Parent.BoundsY) * 100f;
            //Log.WriteLine($"Chunk '{Parent.Position}' has a sparsity of {percentage}% ");
            return new ChunkSparsity
            {
                MiniumHeight = sparsity.X,
                MaximumHeight = sparsity.Y
            };
        }
    }
}