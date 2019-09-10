using System.Diagnostics;
using OpenTK;

namespace Hedra.Engine.Rendering.Isosurface
{
    public class RegularCell
    {
        public const int BLOCK_WIDTH = 16;
        public byte CaseIndex;
        public int[] Verts = new int[4];
    };

    public class RegularCellCache
    {

        public RegularCellCache()
        {
            _cache = new RegularCell[2][];
            for (var i = 0; i < 2; i++)
            {
                _cache[i] = new RegularCell[RegularCell.BLOCK_WIDTH * RegularCell.BLOCK_WIDTH];
                for (var k = 0; k < _cache[i].Length; k++)
                {
                    _cache[i][k] = new RegularCell();
                }
            }
        }
        public RegularCell this[int x, int y, int z] => _cache[z & 1][y * RegularCell.BLOCK_WIDTH + x];

        public RegularCell this[Vector3i p] => this[p.X, p.Y, p.Z];

        private readonly RegularCell[][] _cache;
    };

    public class TransitionCell
    {
        public byte CaseIndex;
        public int[] Verts = new int[10];
    }
    
    public class TransitionCellCache
    {
        private readonly TransitionCell[][] _cache;
        
        public TransitionCellCache()
        {
            _cache = new TransitionCell[2][];
            for (var i = 0; i < 2; i++)
            {
                _cache[i] = new TransitionCell[RegularCell.BLOCK_WIDTH * RegularCell.BLOCK_WIDTH];
                for (var k = 0; k < _cache[i].Length; k++)
                {
                    _cache[i][k] = new TransitionCell();
                }
            }
        }
        
        public TransitionCell this[int X, int Y] => _cache[Y & 1][X];
    };

    
}