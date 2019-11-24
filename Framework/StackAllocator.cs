using System;

namespace Hedra.Framework
{
    public unsafe class StackAllocator : Allocator
    {
        private void* _buffer;
        
        public StackAllocator(int BufferSize, byte* Buffer) : base(BufferSize)
        {
            _buffer = Buffer;
        }

        protected override void* CreateBuffer() => _buffer;

        protected override void FreeBuffer()
        {
            _buffer = null;
            /* Stack memory does not need to be freed */
        }
    }
}