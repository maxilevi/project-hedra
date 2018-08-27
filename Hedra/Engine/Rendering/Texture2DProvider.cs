using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class Texture2DProvider : ITexture2DProvider
    {
        public uint LoadTexture(Bitmap Bmp, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
        {
            var id = Renderer.GenTexture();
            Renderer.BindTexture(TextureTarget.Texture2D, id);
            var bmpData = Bmp.LockBits(new Rectangle(0,0,Bmp.Width, Bmp.Height), ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
	
            Bmp.UnlockBits(bmpData);
	
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Min);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Mag);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Wrap);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Wrap);

            Bmp.Dispose();
            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Loading Texture: " + error, LogType.GL);
            return id;
        }
    }
}