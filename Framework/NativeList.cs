using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hedra.Framework
{
    [DebuggerStepThrough]
    public class NativeList<T> : IDisposable, ISequentialList<T>, IList<T> where T : unmanaged
    {
        private const int InitialSize = 32;
        private readonly IAllocator _allocator;
        private NativeArray<T> _array;
        private int _count;
        private bool _disposed;
        
        public NativeList(IAllocator Allocator) : this(Allocator, null)
        {
        }
        public NativeList(IAllocator Allocator, ICollection<T> Collection)
        {
            _allocator = Allocator;
            _array = new NativeArray<T>(_allocator, Math.Max(InitialSize, Collection?.Count ?? 0));
            _count = 0;
            if(Collection != null)
                AddRange(Collection);
        }

        public void Add(T Object)
        {
            EnsureCapacity(_count + 1);
            _array[_count++] = Object;
        }

        public unsafe void AddRange(NativeArray<T> Array, int Take)
        {
            if (Take < 0 || Take > Array.Length) throw new ArgumentOutOfRangeException(nameof(Take));
            if (Take == 0) return;
            var sameSource = Array.Pointer == _array.Pointer;
            EnsureCapacity(checked(_count + Take));
            var source = (void*)(sameSource ? _array.Pointer : Array.Pointer);
            var destination = (T*)_array.Pointer + _count;
            var bytes = (long)Take * sizeof(T);
            Buffer.MemoryCopy(source, destination, bytes, bytes);
            _count += Take;
        }
        
        public unsafe void AddRange(T[] Array, int Take)
        {
            if (Array == null) throw new ArgumentNullException(nameof(Array));
            if (Take < 0 || Take > Array.Length) throw new ArgumentOutOfRangeException(nameof(Take));
            if (Take == 0) return;
            EnsureCapacity(checked(_count + Take));
            fixed (T* source = Array)
            {
                var bytes = (long)Take * sizeof(T);
                Buffer.MemoryCopy(source, (T*)_array.Pointer + _count, bytes, bytes);
            }
            _count += Take;
        }

        public unsafe void AddRange(IEnumerable<T> Array)
        {
            if (Array is NativeList<T> native)
            {
                AddRange(native.InternalArray, native.Count);
                return;
            }
            if (Array is NativeArray<T> nativeArray)
            {
                AddRange(nativeArray, nativeArray.Length);
                return;
            }
            if (Array is T[] managedArray)
            {
                if (managedArray.Length == 0) return;
                EnsureCapacity(checked(_count + managedArray.Length));
                fixed (T* source = managedArray)
                {
                    var bytes = (long)managedArray.Length * sizeof(T);
                    Buffer.MemoryCopy(source, (T*)_array.Pointer + _count, bytes, bytes);
                }
                _count += managedArray.Length;
                return;
            }
            if (Array is List<T> managedList)
            {
                if (managedList.Count == 0) return;
                EnsureCapacity(checked(_count + managedList.Count));
                fixed (T* source = CollectionsMarshal.AsSpan(managedList))
                {
                    var bytes = (long)managedList.Count * sizeof(T);
                    Buffer.MemoryCopy(source, (T*)_array.Pointer + _count, bytes, bytes);
                }
                _count += managedList.Count;
                return;
            }
            if (Array is ICollection<T> collection) EnsureCapacity(checked(_count + collection.Count));
            foreach (var element in Array)
            {
                Add(element);
            }
        }

        private void EnsureCapacity(int Size)
        {
            if (Size <= _array.Length) return;
            Resize(Math.Max(Size, checked(_array.Length * 2)));
        }

        public NativeList<T> Clone()
        {
            var list = new NativeList<T>(_allocator);
            list.AddRange(this);
            return list;
        }
        
        private unsafe void Resize(int NewSize)
        {
            _array.Resize(NewSize);
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _array[I];
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _array[I] = value;
        }

        public void Set(ICollection<T> Collection)
        {
            Clear();
            AddRange(Collection);
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

        public NativeArray<T> InternalArray => _array;
        
        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();
        
        public bool IsReadOnly => false;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _array.Dispose();
        }
    }
}
