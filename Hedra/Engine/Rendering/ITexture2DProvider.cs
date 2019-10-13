using System.Drawing;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Rendering
{
    public interface ITexture2DProvider
    {
        uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap);
    }
}