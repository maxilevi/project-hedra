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
            _data = Allocator.Get<T>(Size);
            _size = Size;
        }

        public T this[int I]
        {
            get
            {
#if DEBUG
                EnsureBounds(I);
#endif
                return *((T*) _data + I);
            }

            set
            {
#if DEBUG
                EnsureBounds(I);
#endif
                *((T*) _data + I) = value;
            }
        }

        private void EnsureBounds(int I)
        {
            if(I < 0 || I >= _size)
                throw new ArgumentOutOfRangeException();
        }

        public IntPtr Pointer => (IntPtr) _data;

        public int Length => _size;

        public void Dispose()
        {
            _allocator.Free(ref _data);
            _size = 0;
            _allocator = null;
        }
    }
}