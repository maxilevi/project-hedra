using System;

namespace Hedra.Framework
{
    public unsafe interface IAllocator : IDisposable
    {
        Pointer Get<T>(int Count);
        void Free(ref Pointer Ptr);
        Pointer Resize<T>(Pointer Ptr, int NewCount) where T : unmanaged;
        void Clear();
    }
}