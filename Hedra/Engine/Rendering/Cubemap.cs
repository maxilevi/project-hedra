using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public class Cubemap
    {
        public uint TextureId { get;}

        public Cubemap (Bitmap[] TextureArray)
        {
            uint texId;
            GL.GenTextures(1, out texId);
            GL.Enable(EnableCap.TextureCubeMap);
            TextureId = texId;

            GL.BindTexture(TextureTarget.TextureCubeMap, TextureId);
            for (int i = 0; i < TextureArray.Length; i++)
            {
                BitmapData data = TextureArray[i].LockBits(
                    new Rectangle(0, 0, TextureArray[i].Width, TextureArray[i].Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D( (TextureTarget) ( (int) TextureTarget.TextureCubeMapPositiveX + i), 0, PixelInternalFormat.Rgba,
                    data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                    data.Scan0);
                
                TextureArray[i].UnlockBits(data);
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
        }
    }
}
