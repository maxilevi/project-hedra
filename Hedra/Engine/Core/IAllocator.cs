using System;

namespace Hedra.Engine.Core
{
    public unsafe interface IAllocator : IDisposable
    {
        void* Get<T>(int Count);
        void Free(ref void* Ptr);
        void Clear();
    }
}