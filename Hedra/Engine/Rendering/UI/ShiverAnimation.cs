using System;
using Hedra.Core;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.UI
{
    public class ShiverAnimation : TextureAnimation<ISimpleTexture>
    {
        protected override void Process(ISimpleTexture Texture, TextureState State)
        {
            var intensity = (float) Math.Pow(Intensity, 3);
            Texture.Position = State.Position + 
                               new Vector2( 
                                   (float) Math.Cos(Time.AccumulatedFrameTime * 50f) * .0025f * intensity,
                                   (float) Math.Sin(Time.AccumulatedFrameTime * 50f) * .0025f * intensity
                                   );
            Texture.Scale = State.Scale * (1 + intensity * 0.05f);
        }
    }
}