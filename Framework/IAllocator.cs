using System;

namespace Hedra.Framework
{
    public unsafe interface IAllocator : IDisposable
    {
        void* Get<T>(int Count);
        void Free(ref void* Ptr);
        void* Resize<T>(void* Ptr, int NewCount) where T : unmanaged;
        void Clear();
    }
}