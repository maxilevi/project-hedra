using System;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering
{
    public class TextBillboard : BaseBillboard
    {
        private readonly Color _color;
        private readonly Font _font;
        private Vector2 _scale;

        public TextBillboard(float Lifetime, string Text, Color TextColor, Font TextFont, Vector3 Position)
            : this(Lifetime, Text, TextColor, TextFont, () => Position)
        {
        }

        public TextBillboard(float Lifetime, string Text, Color TextColor, Font TextFont, Func<Vector3> Follow)
            : base(Lifetime, Follow)
        {
            _color = TextColor;
            _font = TextFont;
            UpdateText(Text);
        }

        protected override Vector2 Measurements => _scale;

        public void UpdateText(string Text)
        {
            var bmp = GUIText.BuildText(Text, _color, _font, out var measurements);
            Executer.ExecuteOnMainThread(() =>
            {
                TextureId = GUIText.LoadText(bmp);
                _scale = measurements.ToRelativeSize();
            });
        }

        /* We don't dispose fonts since they are from FontCache.cs */
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}