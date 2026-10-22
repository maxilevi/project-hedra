using System;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    internal readonly struct TextRenderKey : IEquatable<TextRenderKey>
    {
        private readonly string _text;
        private readonly FontKey _font;
        private readonly Color _color;
        private readonly bool _stroke;
        private readonly float _strokeWidth;
        private readonly Color _strokeColor;
        private readonly float _rasterScale;

        public TextRenderKey(string Text, Font Font, Color Color, TextOptions Options, float RasterScale)
            : this(Text, Font, Color, Options.HasStroke, Options.StrokeWidth, Options.StrokeColor, RasterScale)
        {
        }

        public TextRenderKey(string Text, Font Font, Color Color, bool Stroke, float StrokeWidth,
            Color StrokeColor, float RasterScale)
        {
            _text = Text;
            _font = new FontKey(Font.Family, Font.Size, Font.Instance.Description.Style);
            _color = Color;
            _stroke = Stroke;
            _strokeWidth = Stroke ? StrokeWidth : 0;
            _strokeColor = Stroke ? StrokeColor : default;
            _rasterScale = RasterScale;
        }

        public bool Equals(TextRenderKey Other)
        {
            return _text == Other._text && _font.Equals(Other._font) && _color.Equals(Other._color) &&
                   _stroke == Other._stroke && _strokeWidth.Equals(Other._strokeWidth) &&
                   _strokeColor.Equals(Other._strokeColor) && _rasterScale.Equals(Other._rasterScale);
        }

        public override bool Equals(object Obj) => Obj is TextRenderKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_text, _font, _color, _stroke,
            _strokeWidth, _strokeColor, _rasterScale);
    }
}
