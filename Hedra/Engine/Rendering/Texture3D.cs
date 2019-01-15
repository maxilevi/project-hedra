using System;
using Hedra.Engine.IO;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class Texture3D : IDisposable
    {
        public uint Id { get; }

        public Texture3D(float[,,] Data)
        {
            Id = Renderer.GenTexture();

            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture3D, Id);
            Renderer.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.Rgb32f, Data.GetLength(0), Data.GetLength(1), Data.GetLength(2), 0,
                PixelFormat.Red, PixelType.Float, Data);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Renderer.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);

            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Texture3D: " + error);
            
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture3D, 0);
        }

        public void Dispose()
        {
            Renderer.DeleteTexture(Id);
        }
    }
}
