using System;

namespace Hedra.Engine.Core
{
    public struct NativeList<T> : IDisposable where T : unmanaged
    {
        private readonly IAllocator _allocator;
        private NativeArray<T> _array;
        private int _count;
        public NativeList(IAllocator Allocator)
        {
            _allocator = Allocator;
            _array = new NativeArray<T>(_allocator, 64);
            _count = 0;
        }

        public void Add(T Object)
        {
            if(_count == _array.Length)
                Resize(_array.Length * 2);
            _array[_count - 1] = Object;
        }

        private void Resize(int NewSize)
        {
            var array = new NativeArray<T>(_allocator, NewSize);
            for (var i = 0; i < _array.Length; ++i)
                array[i] = _array[i];
            _array.Dispose();
        }

        public int Count => _count;

        public void Dispose()
        {
            _array.Dispose();
        }
    }
}