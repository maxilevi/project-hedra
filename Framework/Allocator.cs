using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hedra.Framework
{
    public abstract unsafe class Allocator : IAllocator
    {
        public const int Megabyte = 1024 * 1024;
        public const int Kilobyte = 1024;
        private SortedList<int, AllocationEntry> _entries;
        private readonly Func<IntPtr> _getBuffer;
        private void* _buffer;
        private int _maxSize;

        protected Allocator(int BufferSize)
        {
            _maxSize = BufferSize;
            _entries = new SortedList<int, AllocationEntry>();
            _getBuffer = () => (IntPtr)_buffer;
        }

        protected abstract void* CreateBuffer();

        protected abstract void FreeBuffer();

        public Pointer Get<T>(int Count)
        {
            if(Count == 0) throw new ArgumentNullException("Cannot allocate 0 bytes");
            EnsureBuffer();
            var size = SizePerElement<T>() * Count;
            var offset = FindSpot(size);
            if(offset + size >= _maxSize)
                throw new OutOfMemoryException($"Native Allocator has ran out of memory trying to allocate '{size / 1024}' KB\nusedMemory ='{UsedMemory / 1024}' KB, freeMemory ='{FreeMemory / 1024}' KB, totalMemory ='{TotalMemory / 1024}' KB");
            var ptr = GetPointer(offset, size);
            _entries.Add(offset, new AllocationEntry(offset, size, ptr));

            return ptr;
        }

        protected virtual Pointer GetPointer(int Offset, int Size)
        {
            return new Pointer(_getBuffer, Offset);
        }

        public Pointer Resize<T>(Pointer Ptr, int NewCount) where T : unmanaged
        {
            if(NewCount == 0) throw new ArgumentNullException("Cannot resize to 0 bytes");
            var entry = GetEntry(Ptr, out var index);
            var perElement = SizePerElement<T>();
            var newSize = perElement * NewCount;
            if (newSize == entry.Length) return Ptr;
            Pointer newPtr;
            /* If there is contiguous space available, use that */
            var nextOffset = index < _entries.Count - 1 ? _entries.Values[index + 1].Offset : _maxSize;
            if (entry.Offset + newSize <= nextOffset)
            {
                // A virtual allocation may fit while a resizable backing buffer
                // still needs to grow. Publish the pointer only after that growth.
                newPtr = GetPointer(entry.Offset, newSize);
                _entries[entry.Offset] = new AllocationEntry(entry.Offset, newSize, newPtr);
            }
            else
            {
                newPtr = (Pointer) Get<T>(NewCount);
                Buffer.MemoryCopy(Ptr.Get(), newPtr.Get(), newSize, entry.Length);
                Free(ref Ptr);
            }

            return newPtr;
        }

        private int FindSpot(int Size)
        {
            var offset = 0;
            for (var i = 0; i < _entries.Count; ++i)
            {
                var entry = _entries.Values[i];
                if (offset + Size < entry.Offset)
                    return offset;
                offset = entry.Offset + entry.Length;
            }
            return offset;
        }

        private static int SizePerElement<T>()
        {
            return ElementSize<T>.Value;
        }

        private static class ElementSize<T>
        {
            public static readonly int Value = Marshal.SizeOf(typeof(T));
        }

        private void EnsureBuffer()
        {
            if (_buffer == null)
                _buffer = CreateBuffer();
        }

        public int UsedMemory
        {
            get
            {
                var mem = 0;
                for (var i = 0; i < _entries.Count; ++i)
                {
                    mem += _entries.Values[i].Length;
                }
                return mem;
            }
        }

        public int FreeMemory => TotalMemory - UsedMemory;

        public int TotalMemory => _maxSize;

        public bool IsEmpty => _entries.Count == 0;

        public void Clear()
        {
            _entries.Clear();
        }

        private unsafe AllocationEntry GetEntry(Pointer Ptr, out int Index)
        {
            for (var i = _entries.Count-1; i > -1; --i)
            {
                Index = i;
                var entry = _entries.Values[i];
                if (entry.Ptr.Same(Ptr)) return entry;
            }
            throw new ArgumentOutOfRangeException("Ptr was not allocated with this allocator.");
        }

        public void Free(ref Pointer Ptr)
        {
            GetEntry(Ptr, out var index);
            _entries.RemoveAt(index);
            Ptr = new Pointer();
        }

        public virtual void Dispose()
        {
            EnsureBuffer();
            _entries.Clear();
            _entries = null;
            _buffer = null;
            FreeBuffer();
        }

        ~Allocator()
        {
            if (_buffer != null)
            {
                Dispose();
            }
        }
    }
}
