using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using System.Linq;
using System.Text;
using SixLabors.Fonts;

namespace Hedra.Engine.Rendering.UI
{
    public class TextConfiguration
    {
        public Color Color { get; set; }
        public Font Font { get; set; }

        public TextConfiguration(Color Color, Font Font)
        {
            this.Color = Color;
            this.Font = Font;
        }
    }
}