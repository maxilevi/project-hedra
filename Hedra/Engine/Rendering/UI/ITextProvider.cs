using System.Drawing;

namespace Hedra.Engine.Rendering.UI
{
    public interface ITextProvider
    {
        Bitmap BuildText(string Text, Font TextFont, Color TextColor);
    }
}