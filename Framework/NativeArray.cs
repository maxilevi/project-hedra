using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hedra.Framework
{
    [DebuggerStepThrough]
    public unsafe struct NativeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        private int _size;
        private Pointer _data;
        private IAllocator _allocator;
        
        public NativeArray(IAllocator Allocator, int Size)
        {
            _allocator = Allocator;
            _data = Allocator.Get<T>(Size);
            _size = Size;
        }

        public void Resize(int NewSize)
        {
            _data = _allocator.Resize<T>(_data, NewSize);
            _size = NewSize;
        }

        public T this[int I]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                EnsureBounds(I);
#endif
                return *((T*) _data.Get() + I);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if DEBUG
                EnsureBounds(I);
#endif
                *((T*) _data.Get() + I) = value;
            }
        }

        private void EnsureBounds(int I)
        {
            if(I < 0 || I >= _size)
                throw new ArgumentOutOfRangeException();
        }

        public IntPtr Pointer => (IntPtr) _data.Get();

        public int Length => _size;

        public void Dispose()
        {
            _allocator.Free(ref _data);
            _size = 0;
            _allocator = null;
        }

        public void Trim(int Size)
        {
            _size = Size;
        }

        private IEnumerable<T> Enumerate()
        {
            for (var i = 0; i < Length; ++i)
            {
                yield return this[i];
            }
        }

        public int OccupiedBytes => _size * Marshal.SizeOf<T>();
        
        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}