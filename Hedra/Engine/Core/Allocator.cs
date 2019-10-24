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
            var ptr = (void*) ((byte*)_buffer + offset);
            _entries.Add(offset, new AllocationEntry(offset, size, ptr));

            return ptr;
        }

        public void* Resize<T>(void* Ptr, int NewCount) where T : unmanaged
        {
            var entry = GetEntry(Ptr, out var index);
            var perElement = SizePerElement<T>();
            var newSize = perElement * NewCount;
            void* newPtr;
            /* If there is contiguous space available, use that */
            var nextOffset = index < _entries.Count - 1 ? ((AllocationEntry) _entries.GetByIndex(index + 1)).Offset : _maxSize;
            if (entry.Offset + newSize < nextOffset)
            {
                _entries.RemoveAt(index);
                _entries.Add(entry.Offset, new AllocationEntry(entry.Offset, newSize, Ptr));
                newPtr = Ptr;
            }
            else
            {
                newPtr = (T*) Get<T>(NewCount);
                Buffer.MemoryCopy(Ptr, newPtr, newSize, entry.Length);
                Free(ref Ptr);
            }

            return newPtr;
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

        public bool IsEmpty => _entries.Count == 0;

        public void Clear()
        {
            _entries.Clear();
        }

        private unsafe AllocationEntry GetEntry(void* Ptr, out int Index)
        {
            for (var i = _entries.Count-1; i > -1; --i)
            {
                Index = i;
                var entry = (AllocationEntry)_entries.GetByIndex(i);
                if (entry.Ptr == Ptr) return entry;
            }
            throw new ArgumentOutOfRangeException("Ptr was not allocated with this allocator.");
        }

        public void Free(ref void* Ptr)
        {
            GetEntry(Ptr, out var index);
            _entries.RemoveAt(index);
            Ptr = null;
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
                if(!Program.GameWindow.IsExiting)
                    throw new ArgumentException("Allocator was not disposed");
            }
        }
    }
}