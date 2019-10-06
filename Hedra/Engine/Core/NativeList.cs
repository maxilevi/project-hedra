using System;
using System.Collections;
using System.Collections.Generic;

namespace Hedra.Engine.Core
{
    public class NativeList<T> : IDisposable, ISequentialList<T>, IList<T> where T : unmanaged
    {
        private const int InitialSize = 32;
        private readonly IAllocator _allocator;
        private NativeArray<T> _array;
        private int _count;
        
        public NativeList(IAllocator Allocator) : this(Allocator, null)
        {
        }
        public NativeList(IAllocator Allocator, ICollection<T> Collection)
        {
            _allocator = Allocator;
            _array = new NativeArray<T>(_allocator, InitialSize);
            _count = 0;
            if(Collection != null)
                AddRange(Collection);
        }

        public void Add(T Object)
        {
            EnsureCapacity(_count);
            _array[_count++] = Object;
        }

        public void AddRange(NativeArray<T> Array, int Take)
        {
            for(var i = 0; i < Take; ++i)
                Add(Array[i]);
        }
        
        public void AddRange(IEnumerable<T> Array)
        {
            foreach (var element in Array)
            {
                Add(element);
            }
        }

        private void EnsureCapacity(int Size)
        {
            if (Size >= _array.Length)
            {
                Resize(_array.Length * 2);
                EnsureCapacity(Size);
            }
        }

        public NativeList<T> Clone()
        {
            var list = new NativeList<T>(_allocator);
            list.EnsureCapacity(_count);
            for (var i = 0; i < _count; ++i)
                list._array[i] = _array[i];
            list._count = _count;
            return list;
        }
        
        private void Resize(int NewSize)
        {
            var array = new NativeArray<T>(_allocator, NewSize);
            for (var i = 0; i < _array.Length; ++i)
                array[i] = _array[i];
            _array.Dispose();
            _array = array;
        }
        
        private void EnsureBounds(int I)
        {
            if(I < 0 || I >= _count)
                throw new ArgumentOutOfRangeException();
        }

        public int IndexOf(T Item)
        {
            for (var i = 0; i < Count; ++i)
            {
                if (this[i].Equals(Item))
                    return i;
            }
            return -1;
        }

        public void Insert(int Index, T Item)
        {
            EnsureCapacity(Count + 1);
            var previous = Item;
            for (var i = Index; i < Count + 1; ++i)
            {
                var aux = this[i];
                this[i] = previous;
                previous = aux;
            }
        }

        public void RemoveAt(int Index)
        {
            throw new NotImplementedException();
        }

        public T this[int I]
        {
            get
            {
#if DEBUG
                EnsureBounds(I);
#endif
                return _array[I];
            }
            set
            {
#if DEBUG
                EnsureBounds(I);
#endif
                _array[I] = value;
            }
        }

        public void Set(ICollection<T> Collection)
        {
            Clear();
            foreach (var element in Collection)
            {
                Add(element);
            }
        }
        
        public void Set(T Value, int Times)
        {
            Clear();
            EnsureCapacity(Times);
            _count = Times;
            for (var i = 0; i < _count; ++i)
                this[i] = Value;
        }

        public void Clear()
        {
            Resize(InitialSize);
            _count = 0;
        }

        public int Count => _count;

        public IntPtr Pointer => _array.Pointer;

        public bool Contains(T Item)
        {
            for (var i = 0; i < _count; ++i)
            {
                if (_array[i].Equals(Item))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] Array, int ArrayIndex)
        {
            for (var i = ArrayIndex; i < Array.Length; ++i)
            {
                Array[i] = this[i];
            }
        }

        public bool Remove(T Item) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => new
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        public bool IsReadOnly => throw new NotImplementedException();

        public void Dispose()
        {
            _array.Dispose();
        }
    }
}