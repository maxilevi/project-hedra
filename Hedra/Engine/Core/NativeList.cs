using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
            EnsureCapacity(((ICollection<T>)Array).Count);
            foreach (var element in Array)
            {
                Add(element);
            }
        }

        private void EnsureCapacity(int Size)
        {
            if (Size < _array.Length) return;
            Resize(Size * 2);
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
            for (var i = 0; i < _count; ++i)
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

        public bool Remove(T Item)
        {
            var index = IndexOf(Item);
            if(index != -1)
                RemoveAt(index);
            return index != -1;
        }

        public T this[int I]
        {
            [MethodImpl(256)]
            get
            {
#if DEBUG
                //EnsureBounds(I);
#endif
                return _array[I];
            }
            [MethodImpl(256)]
            set
            {
#if DEBUG
                //EnsureBounds(I);
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
            _count = 0;
            Resize(InitialSize);
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
            for (var i = 0; i < _count; ++i)
            {
                Array[i + ArrayIndex] = this[i];
            }
        }

        private IEnumerable<T> Enumerate()
        {
            for (var i = 0; i < _count; ++i)
            {
                yield return this[i];
            }
        }
        
        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();
        public bool IsReadOnly => false;

        public void Dispose()
        {
            _array.Dispose();
        }
    }
}