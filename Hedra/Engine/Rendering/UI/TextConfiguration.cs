using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public class TextConfiguration
    {
        public TextConfiguration(Color Color, Font Font)
        {
            this.Color = Color;
            this.Font = Font;
        }

        public Color Color { get; set; }
        public Font Font { get; set; }
    }
}