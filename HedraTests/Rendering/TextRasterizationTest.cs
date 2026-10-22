using System;
using System.Collections.Generic;
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
            TextProvider.ClearBitmapCache();
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

        [Test]
        public void CachedBitmapsRemainIndependentOfCallerMutationAndDisposal()
        {
            TextProvider.ClearBitmapCache();
            var provider = new TextProvider();
            var font = FontCache.GetNormal(18);
            const string text = "cached bitmap ownership";
            using var expected = new ReferenceTextProvider().BuildText(text, font, Color.White);
            using (var first = provider.BuildText(text, font, Color.White)) first[0, 0] = Color.Magenta;
            using var second = provider.BuildText(text, font, Color.White);
            Assert.True(expected.TryGetSinglePixelSpan(out var expectedPixels));
            Assert.True(second.TryGetSinglePixelSpan(out var actualPixels));
            Assert.True(expectedPixels.SequenceEqual(actualPixels));
            Assert.AreEqual(1, TextProvider.CachedBitmapCount);
            TextProvider.ClearBitmapCache();
            // Clearing the cache must not dispose the image owned by the caller.
            Assert.AreEqual(expected[0, 0], second[0, 0]);
        }

        [Test]
        public void BitmapCacheEvictsEntriesWithinItsMemoryAndCountBudgets()
        {
            TextProvider.ClearBitmapCache();
            var provider = new TextProvider();
            var font = FontCache.GetNormal(6);
            for (var i = 0; i < TextProvider.MaxCachedBitmaps + 3; i++)
            {
                using var image = provider.BuildText("entry " + i, font, Color.White);
                Assert.LessOrEqual(TextProvider.CachedBitmapCount, TextProvider.MaxCachedBitmaps);
                Assert.LessOrEqual(TextProvider.CachedBitmapBytes, TextProvider.MaxCachedBitmapBytes);
            }
            Assert.AreEqual(TextProvider.MaxCachedBitmaps, TextProvider.CachedBitmapCount);
            TextProvider.ClearBitmapCache();
            Assert.AreEqual(0, TextProvider.CachedBitmapCount);
            Assert.AreEqual(0, TextProvider.CachedBitmapBytes);
        }

        [Test]
        public void BitmapCacheSeparatesFontsColorsAndMutableStrokeOptions()
        {
            var provider = new TextProvider();
            var options = new TextOptions();
            const string text = "cache rendering inputs";
            foreach (var font in new[] { FontCache.GetNormal(16), FontCache.GetBold(16), FontCache.GetNormal(18) })
            foreach (var color in new[] { Color.White, Color.Red, Color.FromRgba(255, 255, 255, 100) })
            for (var stroke = 0; stroke < 3; stroke++)
            {
                options.HasStroke = stroke != 0;
                options.StrokeWidth = stroke == 1 ? 1.75f : 3;
                options.StrokeColor = stroke == 1 ? Color.Black : Color.Blue;
                using var expected = new ReferenceTextProvider().BuildText(text, font, color, options);
                using var actual = provider.BuildText(text, font, color, options);
                Assert.AreEqual(expected.Size(), actual.Size());
                Assert.True(expected.TryGetSinglePixelSpan(out var expectedPixels));
                Assert.True(actual.TryGetSinglePixelSpan(out var actualPixels));
                Assert.True(expectedPixels.SequenceEqual(actualPixels));
            }
        }

        [Test]
        public void BitmapCacheEvictsLargeImagesBeforeExceedingTheByteBudget()
        {
            TextProvider.ClearBitmapCache();
            var provider = new TextProvider();
            var font = FontCache.GetNormal(18);
            var text = new string('M', 1024);
            for (var i = 0; i < 5; i++)
            {
                using var image = provider.BuildText(text + i, font, Color.White);
                Assert.LessOrEqual(TextProvider.CachedBitmapBytes, TextProvider.MaxCachedBitmapBytes);
            }
            Assert.Less(TextProvider.CachedBitmapCount, 5, "The byte budget must evict images before the entry limit");
            TextProvider.ClearBitmapCache();
        }

        public static IEnumerable<TestCaseData> EquivalenceCases()
        {
            foreach (var text in new[] { "Health: 123 / 456", "$(BOLD)(ORANGE){Quest} $(GREEN){ready}",
                "a$(RED){a}a$(BIGGER){BIG}$(SMALLER){small}", "\nfirst\r\n\nlast", "", "  spaces  ",
                "$(CAPS){mixed Case}", "$(RED){literal RED} trailing", "malformed ${format", "café Ελληνικά 中文" })
            foreach (var scale in new[] { .5f, 1f, 1.5f })
            foreach (var stroke in new[] { false, true })
                yield return new TestCaseData(text, scale, stroke);
        }

        [TestCaseSource(nameof(EquivalenceCases))]
        public void OptimizedRendererMatchesReferencePixels(string Text, float Scale, bool Stroke)
        {
            GameSettings.UIScaling = Scale;
            var font = FontCache.GetNormal(18);
            var options = new TextOptions { HasStroke = Stroke, StrokeWidth = 2.5f,
                StrokeColor = Color.FromRgba(40, 80, 120, 200) };
            var color = Color.FromRgba(180, 210, 220, 170);
            var expectedParams = ReferenceTextProvider.BuildParams(Text, font, color, options);
            var actualParams = TextProvider.BuildParams(Text, font, color, options);
            Assert.AreEqual(expectedParams.Texts, actualParams.Texts);
            Assert.AreEqual(expectedParams.TextFonts, actualParams.TextFonts);
            Assert.AreEqual(expectedParams.TextColors, actualParams.TextColors);
            Assert.AreEqual(expectedParams.Offsets, actualParams.Offsets);
            using var expected = new ReferenceTextProvider().BuildText(Text, font, color, options);
            using var actual = new TextProvider().BuildText(Text, font, color, options);
            Assert.AreEqual(expected.Size(), actual.Size());
            Assert.True(expected.TryGetSinglePixelSpan(out var expectedPixels));
            Assert.True(actual.TryGetSinglePixelSpan(out var actualPixels));
            Assert.True(expectedPixels.SequenceEqual(actualPixels), "Rasterization must preserve every RGBA pixel");
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
