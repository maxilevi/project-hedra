using System.Drawing;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using System;

namespace HedraTests
{
    public class SimpleTexture2DProviderMock : ITexture2DProvider
    {
        public uint LoadTexture(Bitmap Bmp, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
        {
            return (uint) Math.Abs(Bmp.GetHashCode());
        }
    }
}