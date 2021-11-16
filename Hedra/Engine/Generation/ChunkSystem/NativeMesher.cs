using System;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Generation.ChunkSystem
{
    public class NativeMesher : IDisposable
    {
        private bool _disposed;
        private IntPtr _ptr;

        public void Dispose()
        {
            _disposed = true;
            //HedraCoreNative.meshing_destroy(_ptr);
        }

        public void CreateCell(ref GridCell Cell, int x, int y, int z, bool isWater, int lod, out bool success)
        {
            //HedraCoreNative
            success = false;
        }

        ~NativeMesher()
        {
            if (!_disposed)
                Dispose();
        }
    }
}