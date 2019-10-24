using System;
using Hedra.Engine.Rendering.Core;
using Hedra.Rendering;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Windowing;
using IronPython.Runtime;

namespace Hedra.Engine.Rendering
{
    public class Texture2D : IDisposable
    {
        public uint Id { get; }
        private bool _disposed;

        public Texture2D(string Path, bool Repeat)
        {
            Id = Graphics2D.LoadFromAssets(Path, TextureMinFilter.Linear, TextureMagFilter.Linear, Repeat ? TextureWrapMode.Repeat : TextureWrapMode.ClampToBorder);
        }

        public unsafe Texture2D(float[] Pixels, int Width, int Height)
        {
            Id = Renderer.GenTexture();

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, Id);
            fixed(float* ptr = Pixels)
                Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, Width, Height, 0, PixelFormat.Red, PixelType.Float, (IntPtr)ptr);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Texture3D: " + error);
            
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
        }
        
        public void Dispose()
        {
            _disposed = true;
            TextureRegistry.Remove(Id);
        }

        ~Texture2D()
        {
            if(!_disposed)
                Dispose();
        }
    }
}