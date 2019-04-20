using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class Cubemap
    {
        public uint TextureId { get;}

        public Cubemap (IList<Bitmap> TextureArray, bool Dispose = true)
        {
            TextureId = Renderer.GenTexture();

            Renderer.BindTexture(TextureTarget.TextureCubeMap, TextureId);
            for (var i = 0; i < TextureArray.Count; i++)
            {
                BitmapData data = TextureArray[i].LockBits(
                    new Rectangle(0, 0, TextureArray[i].Width, TextureArray[i].Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Renderer.TexImage2D( (TextureTarget) ( (int) TextureTarget.TextureCubeMapPositiveX + i), 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte,
                    data.Scan0);
                
                TextureArray[i].UnlockBits(data);
                if(Dispose) TextureArray[i].Dispose();
            }
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToBorder);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToBorder);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToBorder);
            
            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Cubemap: " + error);
            
            Renderer.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public void Bind()
        {
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.TextureCubeMap, TextureId);
        }

        public void Unbind()
        {
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.TextureCubeMap, 0);
        }
    }
}
