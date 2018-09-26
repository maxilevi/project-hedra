using System.Drawing;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL4;
using System;

namespace HedraTests
{
    public class SimpleTexture2DProviderMock : ITexture2DProvider
    {
        public uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min, TextureMagFilter Mag, TextureWrapMode Wrap)
        {
            return (uint) Math.Abs(BitmapObject.GetHashCode());
        }
    }
}