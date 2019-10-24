using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Core
{
    public unsafe class HeapAllocator : Allocator
    {
        private void* _buffer;
        public HeapAllocator(int BufferSize) : base(BufferSize)
        {
            MemoryPool<byte>.Shared.Rent(BufferSize);
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