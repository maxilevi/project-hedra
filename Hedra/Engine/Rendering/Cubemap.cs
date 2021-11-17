using System;
using System.Collections.Generic;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hedra.Engine.Rendering
{
    public class Cubemap
    {
        public Cubemap(IList<Image<Rgba32>> TextureArray, bool Dispose = true)
        {
            TextureId = Renderer.GenTexture();

            Renderer.BindTexture(TextureTarget.TextureCubeMap, TextureId);
            for (var i = 0; i < TextureArray.Count; i++)
            {
                if (!TextureArray[i].TryGetSinglePixelSpan(out var pixelSpan))
                    throw new ArgumentException("Image is not contigous");

                Renderer.TexImage2D((TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i), 0,
                    PixelInternalFormat.Rgba,
                    TextureArray[i].Width, TextureArray[i].Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte,
                    pixelSpan.AsIntPtr());
                
                if (Dispose) TextureArray[i].Dispose();
            }

            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToBorder);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToBorder);
            Renderer.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
                (int)TextureWrapMode.ClampToBorder);

            var error = Renderer.GetError();
            if (error != ErrorCode.NoError)
                Log.WriteLine("GL Error: Cubemap: " + error);

            Renderer.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public uint TextureId { get; }

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