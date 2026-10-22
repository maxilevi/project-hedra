using System;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Rendering.UI;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HedraTests.Rendering
{
    [TestFixture, NonParallelizable]
    public class TextRasterizationTest
    {
        private int _height;
        private float _scale;

        [SetUp]
        public void Setup()
        {
            _height = GameSettings.Height;
            _scale = GameSettings.UIScaling;
            GameSettings.Height = 1080;
            GameSettings.UIScaling = 1;
        }

        [TearDown]
        public void Teardown()
        {
            GameSettings.Height = _height;
            GameSettings.UIScaling = _scale;
        }

        [TestCase(.5f, 1080)]
        [TestCase(2f, 1080)]
        [TestCase(1f, 720)]
        [TestCase(1f, 2160)]
        public void ScalingChangesGlyphsAsWellAsCanvas(float Scale, int Height)
        {
            var provider = new TextProvider();
            var font = FontCache.GetNormal(20);
            const string text = "MMMMMMMMMMMMMMMM";
            using var baseline = provider.BuildText(text, font, Color.White);
            var original = InkSize(baseline);
            GameSettings.Height = Height;
            GameSettings.UIScaling = Scale;
            using var scaled = provider.BuildText(text, font, Color.White);
            var ink = InkSize(scaled);
            var expected = Scale * Height / 1080f;
            Assert.That(ink.Width / original.Width, Is.EqualTo(expected).Within(.06));
            Assert.That(ink.Height / original.Height, Is.EqualTo(expected).Within(.15));
            AssertTransparentBorder(scaled);
        }

        [Test]
        public void MixedSizeMultilineTextUsesTheSameLayoutForMeasurementAndDrawing()
        {
            const string text = "$(BIGGER){BIG}\n\nsmall\r\n$(SMALLER){last}";
            var font = FontCache.GetNormal(24);
            var options = new TextOptions { HasStroke = true, StrokeWidth = 4 };
            using var bitmap = new TextProvider().BuildText(text, font, Color.White, options);
            var size = TextProvider.CalculateNeededSize(TextProvider.BuildParams(text, font, Color.White, options));
            Assert.AreEqual((int)Math.Ceiling(size.Width), bitmap.Width);
            Assert.AreEqual((int)Math.Ceiling(size.Height), bitmap.Height);
            AssertTransparentBorder(bitmap);
            using var withoutBlank = new TextProvider().BuildText(text.Replace("\n\n", "\n"), font, Color.White, options);
            Assert.Greater(bitmap.Height, withoutBlank.Height);
        }

        [Test]
        public void StrokeWidthChangesTheRenderedOutline()
        {
            var provider = new TextProvider();
            var font = FontCache.GetNormal(24);
            using var thin = provider.BuildText("outline", font, Color.White,
                new TextOptions { HasStroke = true, StrokeWidth = 1 });
            using var thick = provider.BuildText("outline", font, Color.White,
                new TextOptions { HasStroke = true, StrokeWidth = 5 });
            Assert.Greater(InkSize(thick).Width, InkSize(thin).Width);
            Assert.Greater(InkSize(thick).Height, InkSize(thin).Height);
            AssertTransparentBorder(thick);
        }

        private static SizeF InkSize(Image<Rgba32> Bitmap)
        {
            var minX = Bitmap.Width;
            var minY = Bitmap.Height;
            var maxX = -1;
            var maxY = -1;
            for (var y = 0; y < Bitmap.Height; y++)
            for (var x = 0; x < Bitmap.Width; x++)
                if (Bitmap[x, y].A != 0)
                {
                    minX = Math.Min(x, minX);
                    maxX = Math.Max(x, maxX);
                    minY = Math.Min(y, minY);
                    maxY = Math.Max(y, maxY);
                }
            return new SizeF(maxX - minX + 1, maxY - minY + 1);
        }

        private static void AssertTransparentBorder(Image<Rgba32> Bitmap)
        {
            for (var x = 0; x < Bitmap.Width; x++)
            {
                Assert.AreEqual(0, Bitmap[x, 0].A, "Top edge is clipped");
                Assert.AreEqual(0, Bitmap[x, Bitmap.Height - 1].A, "Bottom edge is clipped");
            }
            for (var y = 0; y < Bitmap.Height; y++)
            {
                Assert.AreEqual(0, Bitmap[0, y].A, "Left edge is clipped");
                Assert.AreEqual(0, Bitmap[Bitmap.Width - 1, y].A, "Right edge is clipped");
            }
        }
    }
}
