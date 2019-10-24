using System.Drawing;
using Hedra.Engine.Rendering;
using Hedra.Engine.Core;
using System;
using Hedra.Engine.Windowing;

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