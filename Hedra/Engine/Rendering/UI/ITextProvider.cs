using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public interface ITextProvider
    {
        Bitmap BuildText(string Text, Font TextFont, Color TextColor);
        Bitmap BuildText(string Text, Font TextFont, Color TextColor, TextOptions Options);
    }
}