using System;
using Hedra.Engine.Rendering.Core;
using Hedra.Rendering;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering
{
    public class Texture : IDisposable
    {
        public uint Id { get; }
        private bool _disposed;

        public Texture(string Path, bool Repeat)
        {
            Id = Graphics2D.LoadFromAssets(Path, TextureMinFilter.Linear, TextureMagFilter.Linear, Repeat ? TextureWrapMode.Repeat : TextureWrapMode.ClampToBorder);
        }
        
        public void Dispose()
        {
            _disposed = true;
            TextureRegistry.Remove(Id);
        }

        ~Texture()
        {
            if(!_disposed)
                Dispose();
        }
    }
}