using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Core
{
    public unsafe struct NativeArray<T> : IDisposable where T : unmanaged
    {
        private int _size;
        private void* _data;
        private IAllocator _allocator;
        
        public NativeArray(IAllocator Allocator, int Size)
        {
            _allocator = Allocator;
            _data = Allocator.Malloc<T>(Size);
            _size = Size;
        }

        public T this[int I]
        {
            get => *((T*)_data + I);

            set => *((T*)_data + I) = value;
        }

        public int Length => _size;

        public void Dispose()
        {
            _allocator.Free(ref _data);
            _size = 0;
            _allocator = null;
        }
    }
}