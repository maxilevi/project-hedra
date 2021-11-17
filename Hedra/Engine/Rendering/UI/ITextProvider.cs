using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hedra.Engine.Rendering.UI
{
    public interface ITextProvider
    {
        Image<Rgba32> BuildText(string Text, Font TextFont, Color TextColor);
        Image<Rgba32> BuildText(string Text, Font TextFont, Color TextColor, TextOptions Options);
    }
}