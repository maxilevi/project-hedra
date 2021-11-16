using System;
using Hedra.Engine.IO;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering.Core
{
    public class Texture3D : IDisposable
    {
        public Texture3D(float[] Data, int Width, int Height, int Depth)
        {
            Id = Renderer.GenTexture();

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture3D, Id);
            Renderer.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.Rgb32f, Width, Height, Depth, 0,
                PixelFormat.Red, PixelType.Float, Data);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.Repeat);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.Repeat);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR,
                (int)TextureWrapMode.Repeat);

            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Texture3D: " + error);

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture3D, 0);
        }

        public uint Id { get; }

        public void Dispose()
        {
            TextureRegistry.Remove(Id);
        }
    }
}