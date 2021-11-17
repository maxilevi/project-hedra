using System;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering
{
    public class Texture2DProvider : ITexture2DProvider
    {
        public uint LoadTexture(BitmapObject BitmapObject, TextureMinFilter Min, TextureMagFilter Mag,
            TextureWrapMode Wrap)
        {
            var bmp = BitmapObject.Bitmap;
            var id = Renderer.GenTexture();
            Renderer.BindTexture(TextureTarget.Texture2D, id);
            if (!bmp.TryGetSinglePixelSpan(out var pixelSpan))
                throw new ArgumentException("Image is not contigous");
            
            Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixelSpan.AsIntPtr());

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