﻿using System.Drawing;
using Hedra.Engine.Rendering.UI;

namespace HedraTests.Rendering
{
    public class SimpleTextProviderMock : ITextProvider
    {
        public Bitmap BuildText(string Text, Font TextFont, Color TextColor)
        {
            return new Bitmap(1, 1);
        }
    }
}