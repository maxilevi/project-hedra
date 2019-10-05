namespace Hedra.Engine.Core
{
    public unsafe interface IAllocator
    {
        void* Malloc<T>(int Count);
        void Free(ref void* Ptr);
    }
}