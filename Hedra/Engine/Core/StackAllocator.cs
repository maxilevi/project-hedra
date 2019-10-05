using System.Runtime.InteropServices;

namespace Hedra.Engine.Core
{
    public unsafe struct StackAllocator : IAllocator
    {
        private void* _buffer;
        private int _offset;
        
        public StackAllocator(void* Buffer)
        {
            _buffer = Buffer;
            _offset = 0;
        }
        
        public void* Malloc<T>(int Count)
        {
            throw new System.NotImplementedException();
        }

        public void Free(ref void* Ptr)
        {
            throw new System.NotImplementedException();
        }
    }
}