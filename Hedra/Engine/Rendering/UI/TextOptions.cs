using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public class TextOptions
    {
        public bool HasStroke { get; set; }
        public float StrokeWidth { get; set; }
        public Color StrokeColor { get; set; } = Color.Black;
    }
}