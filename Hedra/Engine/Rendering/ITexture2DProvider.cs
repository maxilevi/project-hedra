using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public interface ITexture2DProvider
    {
        uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap);
    }
}