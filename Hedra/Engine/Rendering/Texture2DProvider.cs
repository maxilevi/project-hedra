using System.Drawing;
using System.Drawing.Imaging;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class Texture2DProvider : ITexture2DProvider
    {
        public uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
        {
            var bmp = BitmapObject.Bitmap;
            var id = Renderer.GenTexture();
            Renderer.BindTexture(TextureTarget.Texture2D, id);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
    
            bmp.UnlockBits(bmpData);
    
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Min);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Mag);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Wrap);
            Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Wrap);

            bmp.Dispose();
            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine($"GL Error: Loading Texture: '{BitmapObject.Path}' " + error);
            return id;
        }
    }
}