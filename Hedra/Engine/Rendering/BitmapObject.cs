using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hedra.Engine.Rendering
{
    public class BitmapObject
    {
        public Image<Rgba32> Bitmap { get; set; }
        public string Path { get; set; }
    }
}