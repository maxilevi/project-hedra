using System;
using System.Runtime.InteropServices;

namespace Hedra.Framework
{
    public unsafe class HeapAllocator : Allocator
    {
        private void* _buffer;
        public HeapAllocator(int BufferSize) : base(BufferSize)
        {
            _buffer = (void*) Marshal.AllocHGlobal(BufferSize);
        }

        protected override void* CreateBuffer() => _buffer;

        protected override void FreeBuffer()
        {
            Marshal.FreeHGlobal((IntPtr) _buffer);
            _buffer = null;
        }
    }
}