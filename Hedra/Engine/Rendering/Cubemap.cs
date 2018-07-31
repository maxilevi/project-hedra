using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class Cubemap
    {
        public uint TextureId { get;}

        public Cubemap (IList<Bitmap> TextureArray, bool Dispose = true)
        {
            GL.GenTextures(1, out uint texId);
            Renderer.Enable(EnableCap.TextureCubeMap);
            TextureId = texId;

            GL.BindTexture(TextureTarget.TextureCubeMap, TextureId);
            for (var i = 0; i < TextureArray.Count; i++)
            {
                BitmapData data = TextureArray[i].LockBits(
                    new Rectangle(0, 0, TextureArray[i].Width, TextureArray[i].Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D( (TextureTarget) ( (int) TextureTarget.TextureCubeMapPositiveX + i), 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                    data.Scan0);
                
                TextureArray[i].UnlockBits(data);
                if(Dispose) TextureArray[i].Dispose();
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToBorder);
        }

        public void Bind()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, TextureId);
        }

        public void Unbind()
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }
    }
}
