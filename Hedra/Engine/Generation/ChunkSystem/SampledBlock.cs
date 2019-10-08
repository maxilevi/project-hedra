using System;
using System.Runtime.CompilerServices;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public struct SampledBlock
    {
        public float Density;
        public BlockType Type;
    }

    public unsafe struct SampledBlockWrapper
    {
        private readonly SampledBlock* _ptr;
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        
        public SampledBlockWrapper(SampledBlock* Ptr, int Width, int Height, int Depth)
        {
            _ptr = Ptr;
            this.Width = Width;
            this.Height = Height;
            this.Depth = Depth;
        }
        
        public SampledBlock* this[int X, int Y, int Z]
        {
            [MethodImpl(256)]
            get
            {
#if DEBUG
                //BoundsCheck(X, Y, Z);
#endif
                return &_ptr[X * Depth * Height + Y * Depth + Z];
            }
        }

        private void BoundsCheck(int X, int Y, int Z)
        {
            if(X < 0 || Y < 0 || Z < 0 || X >= Width || Y >= Height || Z >= Depth)
                throw new ArgumentOutOfRangeException();
        }
    }
}