using System;
using System.Runtime.InteropServices;

namespace Hedra.Engine.Rendering.MeshOptimizer
{
    internal class Pointer
    {
        private GCHandle _handle;

        private Pointer()
        {
        }

        public IntPtr Address { get; private set; }

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