using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Core
{
    public abstract unsafe class Allocator : IAllocator
    {
        public const int Megabyte = 1024 * 1024;
        public const int Kilobyte = 1024;
        private SortedList _entries;
        private void* _buffer;
        private int _maxSize;

        protected Allocator(int BufferSize)
        {
            _maxSize = BufferSize;
            _entries = new SortedList();
        }

        protected abstract void* CreateBuffer();

        protected abstract void FreeBuffer();

        public void* Get<T>(int Count)
        {
            EnsureBuffer();
            var size = SizePerElement<T>() * Count;
            var offset = FindSpot(size);
            if(offset + size >= _maxSize)
                throw new OutOfMemoryException($"Native Allocator has ran out of memory trying to allocate '{size / 1024}' KB\nusedMemory ='{UsedMemory / 1024}' KB, freeMemory ='{FreeMemory / 1024}' KB, totalMemory ='{TotalMemory / 1024}' KB");
            var k = SizePerElement<T>();
            var ptr = (void*) ((byte*)_buffer + offset);
            _entries.Add(offset, new AllocationEntry(offset, size, ptr));

            return ptr;
        }

        private int FindSpot(int Size)
        {
            var offset = 0;
            for (var i = 0; i < _entries.Count; ++i)
            {
                var entry = (AllocationEntry)_entries.GetByIndex(i);
                if (offset + Size < entry.Offset)
                    return offset;
                offset = entry.Offset + entry.Length;
            }
            return offset;
        }

        private static int SizePerElement<T>()
        {
            return Marshal.SizeOf(typeof(T));
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
                    mem += ((AllocationEntry)_entries.GetByIndex(i)).Length;
                }
                return mem;
            }
        }

        public int FreeMemory => TotalMemory - UsedMemory;

        public int TotalMemory => _maxSize;

        public void Clear()
        {
            _entries.Clear();
        }

        public void Free(ref void* Ptr)
        {
            for (var i = _entries.Count-1; i > -1; --i)
            {
                var entry = (AllocationEntry)_entries.GetByIndex(i);
                if (entry.Ptr == Ptr)
                {
                    _entries.RemoveAt(i);
                    Ptr = null;
                }
            }
            if(Ptr != null) throw new ArgumentOutOfRangeException("Ptr was not allocated with this allocator.");
        }

        public virtual void Dispose()
        {
            EnsureBuffer();
            _entries.Clear();
            _entries = null;
            _buffer = null;
            FreeBuffer();
        }
    }
}