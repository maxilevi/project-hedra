using System.Runtime.CompilerServices;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public static class SurfaceCellMask
    {
        // Each bit is one positive density sample along a 33-sample Z row.
        // A cell is active when its eight corners are neither all positive nor all nonpositive.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromRows(ulong LowerLeft, ulong LowerRight, ulong UpperLeft, ulong UpperRight,
            int HorizontalStep, uint StrideMask)
        {
            var any = LowerLeft | LowerRight | UpperLeft | UpperRight;
            var all = LowerLeft & LowerRight & UpperLeft & UpperRight;
            return (uint)((any | (any >> HorizontalStep)) & ~(all & (all >> HorizontalStep))) & StrideMask;
        }
    }
}
