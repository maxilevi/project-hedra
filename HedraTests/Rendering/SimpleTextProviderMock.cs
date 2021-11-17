
using SixLabors.ImageSharp;
using Hedra.Engine.Rendering.UI;
using SixLabors.ImageSharp.PixelFormats;
using Color = SixLabors.ImageSharp.Color;
using Font = SixLabors.Fonts.Font;
using Image = SixLabors.ImageSharp.Image;

namespace HedraTests.Rendering
{
    public class SimpleTextProviderMock : ITextProvider
    {
        public Image<Rgba32> BuildText(string Text, Font TextFont, Color TextColor)
        {
            return new Image<Rgba32>(1, 1);
        }

        public Image<Rgba32> BuildText(string Text, Font TextFont, Color TextColor, TextOptions Options)
        {
            return new Image<Rgba32>(1, 1);
        }
    }
}