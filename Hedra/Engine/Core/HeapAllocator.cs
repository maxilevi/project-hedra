using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Core
{
    public unsafe struct HeapAllocator : IAllocator
    {
        public void* Malloc<T>(int Count)
        {
            return (void*) Marshal.AllocHGlobal(SizePerElement<T>() * Count);
        }

        public int SizePerElement<T>()
        {
            return Marshal.SizeOf(typeof(T));
        }

        public void Free(ref void* Ptr)
        {
            Marshal.FreeHGlobal((IntPtr)Ptr);
            Ptr = null;
        }
    }
}