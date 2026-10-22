// Frozen pre-optimization parser and rasterizer from 695dc698 for pixel equivalence and timing.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Hedra.Game;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Hedra.Engine.Rendering.UI
{
    internal class ReferenceTextProvider : ITextProvider
    {
        public const int DefaultDPI = 96;
        public static float RenderingScale => GameSettings.Height / 1080f * GameSettings.UIScaling;
        private static readonly Dictionary<string, Color> ColorMap;
        private static readonly Dictionary<string, Font> FontMap;
        private static readonly Dictionary<string, float> SizeMap;
        private static readonly Dictionary<string, Func<string, string>> TransformationMap;
        private static readonly object MeasureLock = new object();

        static ReferenceTextProvider()
        {
            ColorMap = new Dictionary<string, Color>
            {
                { TextFormatting.Red, Color.Red },
                { TextFormatting.Violet, Color.MediumVioletRed },
                { TextFormatting.Blue, Color.CornflowerBlue },
                { TextFormatting.White, Color.White },
                { TextFormatting.Green, Color.LawnGreen },
                { TextFormatting.Orange, Color.OrangeRed },
                { TextFormatting.Gold, Color.Gold },
                { TextFormatting.Gray, Color.LightGray },
                { TextFormatting.Black, Color.Black },
                { TextFormatting.Yellow, Color.Yellow },
                { TextFormatting.Cyan, Color.Cyan },
                { TextFormatting.Pastel, Color.Wheat }
            };
            FontMap = new Dictionary<string, Font>
            {
                { TextFormatting.Bold, FontCache.GetBold(1) },
                { TextFormatting.Normal, FontCache.GetNormal(1) }
            };
            SizeMap = new Dictionary<string, float>
            {
                { TextFormatting.Smaller, .8f },
                { TextFormatting.Bigger, 1.25f }
            };
            TransformationMap = new Dictionary<string, Func<string, string>>
            {
                { TextFormatting.Caps, S => S.ToUpperInvariant() }
            };
        }

        public Image<Rgba32> BuildText(string Text, Font TextFont, Color TextColor, TextOptions Options)
        {
            return DoBuildText(BuildParams(Text, TextFont, TextColor, Options));
        }

        public Image<Rgba32> BuildText(string Text, Font TextFont, Color TextColor)
        {
            return BuildText(Text, TextFont, TextColor, new TextOptions());
        }

        private static string[] GetSplits(string Text)
        {
            return Regex.Split(Text, @"(\$.+?{.*?})")
                .SelectMany(S => Regex.Split(S, @"(\r\n|\n)"))
                .Where(S => !string.IsNullOrEmpty(S))
                .ToArray();
        }

        public static TextParams BuildParams(string Text, Font TextFont, Color TextColor)
        {
            return BuildParams(Text, TextFont, TextColor, new TextOptions());
        }

        public static TextParams BuildParams(string Text, Font TextFont, Color TextColor, TextOptions Options)
        {
            var splits = GetSplits(Text);
            var texts = splits.Select(StringMatch).ToArray();
            var fullText = string.Join(string.Empty, texts);
            return new TextParams(
                texts,
                texts.Select(S => fullText.IndexOf(S, StringComparison.Ordinal)).ToArray(),
                splits.Select(S => FontMatch(S, TextFont)).ToArray(),
                splits.Select(S => ColorMatch(S, TextColor)).ToArray(),
                splits.Select(S => Options).ToArray()
            );
        }

        private static void Match(string Text, Color Default, Font DefaultFont, out Color Color,
            out string CleanVersion, out Font Font)
        {
            CleanVersion = Text;
            Font = DefaultFont;
            Color = Default;
            const string regex = @"\$(.+?){.*?}";
            if (Regex.IsMatch(Text, regex))
            {
                Color = Replace(
                    ref Text,
                    Default,
                    ColorMap,
                    C => C
                );
                Font = Replace(
                    ref Text,
                    DefaultFont,
                    FontMap,
                    F => FontCache.Get(F, DefaultFont.Size)
                );
                var lambdaFont = Font;
                Font = Replace(
                    ref Text,
                    Font,
                    SizeMap,
                    U => FontCache.Get(lambdaFont, lambdaFont.Size * U)
                );
                Text = Replace(
                    ref Text,
                    Text,
                    TransformationMap,
                    U => U(Text)
                );
                CleanVersion = Regex.Replace(Text, @"\$|\(\)|{|}", string.Empty);
            }
        }

        private static T Replace<T, U>(ref string Text, T Default, Dictionary<string, U> Map, Func<U, T> Do)
        {
            foreach (var pair in Map)
            {
                var regex = $@"{pair.Key}";
                if (!Regex.IsMatch(Text, regex)) continue;
                Text = Regex.Replace(Text, regex, string.Empty);
                return Do(pair.Value);
            }

            return Default;
        }

        private static Font FontMatch(string Text, Font TextFont)
        {
            Match(Text, Color.Transparent, TextFont, out _, out _, out var font);
            return font;
        }

        private static Color ColorMatch(string Text, Color Default)
        {
            Match(Text, Default, FontCache.Default, out var color, out _, out _);
            return color;
        }

        private static string StringMatch(string Text)
        {
            Match(Text, Color.Transparent, FontCache.Default, out _, out var cleaned, out _);
            return cleaned;
        }

        private static Image<Rgba32> DoBuildText(TextParams Params)
        {
            if (Params.TextFonts.Any(F => F.Size > 128) || Params.Texts.Length == 0) return new Image<Rgba32>(1, 1);
            Image<Rgba32> textBitmap;
            lock (MeasureLock)
            {
                textBitmap = DoBuildTextSync(Params);
            }

            return textBitmap;
        }

        private static Image<Rgba32> DoBuildTextSync(TextParams Params)
        {
            var layout = BuildLayout(Params);
            var bitmap = new Image<Rgba32>((int)Math.Ceiling(layout.Size.Width),
                (int)Math.Ceiling(layout.Size.Height));
            for (var i = 0; i < Params.Texts.Length; ++i)
            {
                if (IsNewLine(Params.Texts[i])) continue;
                var options = Params.TextOptions[i];
                var offset = layout.Offsets[i];
                var font = layout.Fonts[i];
                if (options.HasStroke)
                    AddStroke(bitmap, Params.Texts[i], font, offset, options, layout.Scale);
                else
                    AddBackground(bitmap, Params.Texts[i], font, Color.FromRgba(0, 0, 0, 90),
                        offset + new Vector2(1.95f * layout.Scale));
                AddBackground(bitmap, Params.Texts[i], font, Params.TextColors[i], offset);
            }
            return bitmap;
        }

        private static bool IsNewLine(string Text) => Text == "\n" || Text == "\r\n";

        private static void AddStroke(Image<Rgba32> Bitmap, string Text, Font Font, Vector2 Offset,
            TextOptions Options, float Scale)
        {
            var width = Math.Max(0, Options.StrokeWidth) * Scale;
            var color = Options.StrokeColor.WithAlpha(80);
            AddBackground(Bitmap, Text, Font, color, Offset + new Vector2(0, width));
            AddBackground(Bitmap, Text, Font, color, Offset + new Vector2(width, 0));
            AddBackground(Bitmap, Text, Font, color, Offset - new Vector2(0, width));
            AddBackground(Bitmap, Text, Font, color, Offset - new Vector2(width, 0));
        }

        private static void AddBackground(Image<Rgba32> Bmp, string Text, Font TextFont, Color TextColor,
            Vector2 Offset)
        {
            var brush = Brushes.Solid(TextColor);
            var pen = Pens.Solid(TextColor, 0.25f);
            var options = new DrawingOptions
            {
                TextOptions =
                {
                    DpiX = DefaultDPI,
                    DpiY = DefaultDPI,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                }
            };
            Bmp.Mutate(X => X.DrawText(options, Text, TextFont, brush, pen, Offset));
        }

        public static SizeF CalculateNeededSize(TextParams Params)
        {
            lock (MeasureLock) return BuildLayout(Params).Size;
        }

        private static TextLayout BuildLayout(TextParams Params)
        {
            var scale = RenderingScale;
            var fonts = Params.TextFonts.Select(F => FontCache.Get(F, F.Size * scale)).ToArray();
            var offsets = new Vector2[Params.Texts.Length];
            var padding = 1.95f * scale + 1;
            if (Params.TextOptions != null)
                foreach (var options in Params.TextOptions)
                    if (options.HasStroke)
                        padding = Math.Max(padding, Math.Max(0, options.StrokeWidth) * scale + 1);
            padding = (float)Math.Ceiling(padding);
            var x = 0f;
            var y = 0f;
            var lineHeight = 0f;
            var maxWidth = 0f;
            for (var i = 0; i < Params.Texts.Length; ++i)
            {
                var renderer = new RendererOptions(fonts[i], DefaultDPI);
                var defaultHeight = TextMeasurer.Measure("Mg", renderer).Height * 1.1f;
                lineHeight = Math.Max(lineHeight, defaultHeight);
                if (IsNewLine(Params.Texts[i]))
                {
                    maxWidth = Math.Max(maxWidth, x);
                    x = 0;
                    y += lineHeight;
                    lineHeight = defaultHeight;
                    continue;
                }
                var bounds = TextMeasurer.Measure(Params.Texts[i], renderer);
                offsets[i] = new Vector2(x + padding, y + padding);
                x += bounds.Width;
                lineHeight = Math.Max(lineHeight, bounds.Height * 1.1f);
            }
            return new TextLayout
            {
                Fonts = fonts,
                Offsets = offsets,
                Scale = scale,
                Size = new SizeF(Math.Max(1, Math.Max(maxWidth, x) + padding * 2),
                    Math.Max(1, y + lineHeight + padding * 2))
            };
        }

        private sealed class TextLayout
        {
            public Font[] Fonts;
            public Vector2[] Offsets;
            public SizeF Size;
            public float Scale;
        }
    }

}
