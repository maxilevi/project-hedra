using System;
using Hedra.Core;
using OpenTK;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class ChunkSparsity
    {
        public int MinimumHeight { get; set; }
        public int MaximumHeight { get; set; }

        private ChunkSparsity() { }

        private static Vector2 CalculateSparsity(Chunk Parent)
        {
            var lowest = (float)Chunk.BoundsY;
            var highest = 0f;
            for (var x = 0; x < Chunk.BoundsX + 1; x++)
            {
                for (var z = 0; z < Chunk.BoundsZ + 1; z++)
                {
                    var position = Parent.Position + new Vector3(x, 0, z) * Chunk.BlockSize;
                    var chunkSpace = World.ToChunkSpace(position);
                    var chunk = World.GetChunkByOffset(chunkSpace);
                    if (chunk == null) continue;// throw new ArgumentOutOfRangeException($"Sparsity needs to be built when all the neighbours exist.");
                    var coordX = Mathf.Modulo(x, Chunk.BoundsX);
                    var coordZ = Mathf.Modulo(z, Chunk.BoundsZ);
                    var l = Lowest(coordX, coordZ, chunk);
                    var h = Highest(coordX, coordZ, chunk);
                    if (l < lowest) lowest = l;
                    if (h > highest) highest = h;
                } 
            }
            return new Vector2(Math.Max(0, lowest-2), Math.Min(highest+2, Chunk.Height-1));
        }

        private static float Highest(int X, int Z, Chunk Parent)
        {
            for (var y = Chunk.BoundsY-1; y > -1; y--)
            {
                var type = Parent.GetBlockAt(X,y,Z).Type;
                if (type != BlockType.Air)
                    return y+1;
            }
            return 0;
        }

        private static float Lowest(int X, int Z, Chunk Parent)
        {
            for (var y = 0; y < Chunk.BoundsY; y++)
            {
                var type = Parent.GetBlockAt(X,y,Z).Type;
                if (type == BlockType.Air || type == BlockType.Water)
                    return y-1;
            }
            return 0;
        }
        
        public static ChunkSparsity From(Chunk Parent)
        {
            var sparsity = CalculateSparsity(Parent);
            var percentage = ((sparsity.Y - sparsity.X) / Chunk.BoundsY) * 100f;
            //Log.WriteLine($"Chunk '{Parent.Position}' has a sparsity of {percentage}% ");
            return new ChunkSparsity
            {
                MinimumHeight = (int)sparsity.X,
                MaximumHeight = (int)sparsity.Y
            };
        }

        public static ChunkSparsity Default { get; } = new ChunkSparsity();
    }
}