using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class Texture3D : IDisposable
    {
        public uint Id { get; }

        public Texture3D(float[,,] Data)
        {
            uint texId;
            GL.GenTextures(1, out texId);
            Id = texId;

            GL.BindTexture(TextureTarget.Texture3D, Id);
            GL.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.One, Data.GetLength(0), Data.GetLength(1), Data.GetLength(2), 0,
                PixelFormat.Red, PixelType.Float, Data);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Texture3D: " + error);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Id);
        }
    }
}
