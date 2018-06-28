using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Hedra.Engine.Rendering.UI
{
    internal class TextConfiguration
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
