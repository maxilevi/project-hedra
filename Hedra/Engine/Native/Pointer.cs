using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Rendering.MeshOptimizer
{
    internal class Pointer
    {
        private GCHandle _handle;
        public IntPtr Address { get; private set; }
        
        private Pointer()
        {
        }
        
        public void Free()
        {
            _handle.Free();
        }

        public static Pointer Create<T>(T Object)
        {
            var pointer = new Pointer
            {
                _handle = GCHandle.Alloc(Object, GCHandleType.Pinned)
            };
            pointer.Address = pointer._handle.AddrOfPinnedObject();
            return pointer;
        }
    }
}