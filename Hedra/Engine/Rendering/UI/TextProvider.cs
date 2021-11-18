using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
    public class TextProvider : ITextProvider
    {
        public const int DefaultDPI = 96;
        private static readonly Dictionary<string, Color> ColorMap;
        private static readonly Dictionary<string, Font> FontMap;
        private static readonly Dictionary<string, float> SizeMap;
        private static readonly Dictionary<string, Func<string, string>> TransformationMap;
        private static readonly object MeasureLock = new object();

        static TextProvider()
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

        public static string Wrap(string Text, int Characters)
        {
            var splits = GetSplits(Text).ToList();
            var texts = splits.Select(StringMatch).ToList();
            var accumulated = 0;
            for (var i = 0; i < splits.Count; ++i)
            {
                accumulated += texts[i].Length;
                if (texts[i] == splits[i] && texts[i] == Environment.NewLine)
                    accumulated = 0;
                if (accumulated > Characters)
                {
                    var measure = Math.Max(1, Characters - accumulated + texts[i].Length);
                    var previousLen = splits[i].Length;
                    var currentLen = 0;
                    if (splits[i] == texts[i])
                    {
                        splits[i] = Utils.FitString(splits[i], measure, measure < Characters);
                        splits[i] = splits[i].Substring(0, splits[i].Length - Environment.NewLine.Length);
                        currentLen = splits[i].Length;
                    }
                    else
                    {
                        /* String has format */
                        var newStr = Utils.FitString(texts[i], measure, measure < Characters);
                        newStr = newStr.Substring(0, newStr.Length - Environment.NewLine.Length);
                        previousLen = texts[i].Length;
                        currentLen = newStr.Length;
                        var subParts = newStr.Split(Environment.NewLine.ToCharArray())
                            .Where(S => !string.IsNullOrEmpty(S))
                            .ToArray();
                        var newSplit = string.Empty;
                        for (var k = 0; k < subParts.Length; k++)
                        {
                            var upperString = splits[i].Contains("(BOLD)") ? splits[i].ToUpperInvariant() : splits[i];
                            newSplit += $"{upperString.Replace(texts[i], subParts[k])}{Environment.NewLine}";
                        }

                        newSplit = newSplit.Substring(0, newSplit.Length - Environment.NewLine.Length);
                        splits[i] = newSplit;
                    }

                    accumulated -= Characters * ((currentLen - previousLen) / Environment.NewLine.Length);
                }
            }

            return string.Join(string.Empty, splits);
        }

        public static string Substr(string Text, int End)
        {
            var splits = GetSplits(Text);
            var texts = splits.Select(StringMatch).ToArray();
            var accumulated = 0;
            var index = 0;
            var builder = new StringBuilder();
            for (var i = 0; i < texts.Length; ++i)
            {
                index = i;
                if (accumulated + texts[i].Length >= End) break;
                accumulated += texts[i].Length;
                builder.Append(splits[i]);
            }

            builder.Append(splits[index].Replace(texts[index], texts[index].Substring(0, End - accumulated)));
            return builder.ToString();
        }

        public static string StripFormat(string Text)
        {
            return string.Join(string.Empty, GetSplits(Text).Select(StringMatch));
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
            var fullString = string.Join(string.Empty, Params.Texts);
            var size = CalculateNeededSize(Params);
            var textBitmap = new Image<Rgba32>((int)Math.Ceiling(Math.Max(size.Width, 1)),
                (int)Math.Ceiling(Math.Max(size.Height, 1)));
            

            var offset = PointF.Empty;
            var bounds = Vector2.Zero;
            for (var i = 0; i < Params.Texts.Length; ++i)
            {
                if (Params.Texts[i] == Environment.NewLine)
                {
                    offset = new PointF(0, offset.Y + bounds.Y);
                    continue;
                }

                /* Draw shadows & strokes */
                if (Params.TextOptions[i].HasStroke)
                    AddStroke(textBitmap, Params.Texts[i], Params.TextFonts[i], offset, Params.TextOptions[i]);
                else
                    AddShadows(textBitmap, Params.Texts[i], Params.TextFonts[i], offset);
                
                AddBackground(textBitmap, Params.Texts[i], Params.TextFonts[i], Params.TextColors[i], offset);
                
                var rectangle = TextMeasurer.Measure(Params.Texts[i], new RendererOptions(Params.TextFonts[i], DefaultDPI));

                bounds = rectangle.Size;
                offset = new PointF(
                    bounds.X + offset.X,
                    offset.Y
                );

            }

            return textBitmap;
        }

        private static void AddStroke(Image<Rgba32> Bmp, string Text, Font TextFont, PointF Offset, TextOptions Options)
        {
            AddBackground(Bmp, Text, TextFont,
                Options.StrokeColor.WithAlpha(80),
                new Vector2(Offset.X, Offset.Y + 1.75f));
            AddBackground(Bmp, Text, TextFont,
                Options.StrokeColor.WithAlpha(80),
                new Vector2(Offset.X + 1.75f, Offset.Y));
            AddBackground(Bmp, Text, TextFont,
                Options.StrokeColor.WithAlpha(80),
                new Vector2(Offset.X, Offset.Y - 1.75f));
            AddBackground(Bmp, Text, TextFont,
                Options.StrokeColor.WithAlpha(80),
                new Vector2(Offset.X - 1.75f, Offset.Y));
        }

        private static void AddShadows(Image<Rgba32> Bmp, string Text, Font TextFont, PointF Offset)
        {
            AddBackground(Bmp, Text, TextFont, Color.FromRgba(0, 0, 0, 90),
                new Vector2(Offset.X + 1.95f, Offset.Y + 1.95f));
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
            var max = SizeF.Empty;
            var sizes = new SizeF[Params.Texts.Length];
            var fullString = string.Join(string.Empty, Params.Texts);
            var dpi = new Vector2(1, 1);

            for (var i = 0; i < Params.Texts.Length; ++i)
            {
                if (Params.Texts[i] == Environment.NewLine) continue;
                var rectangle = TextMeasurer.Measure(fullString, new RendererOptions(Params.TextFonts[i], DefaultDPI)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                });
                sizes[i] = new SizeF(rectangle.Width, rectangle.Height * 1.1f);
            }

            var bounds = SizeF.Empty;
            var offset = SizeF.Empty;
            for (var i = 0; i < sizes.Length; ++i)
            {
                /*if (Params.Texts[i] == Environment.NewLine && i != sizes.Length - 1)
                {
                    max = new SizeF(max.Width, max.Height + bounds.Height);
                    offset = SizeF.Empty;
                }
                else
                {
                    bounds = sizes[i];
                    offset += bounds;*/
                max = new SizeF(Math.Max(sizes[i].Width, max.Width), Math.Max(max.Height, sizes[i].Height));
                //max = new SizeF(Math.Max(offset.Width, max.Width), Math.Max(max.Height, bounds.Height));
                //}
            }

        var factor = 1f / 1080f * GameSettings.Height;
            return new SizeF(max.Width * factor * GameSettings.UIScaling,
                max.Height * factor * GameSettings.UIScaling);
        }
    }

    public class TextParams
    {
        public TextParams(string[] Texts, int[] Offsets, Font[] TextFonts, Color[] TextColors,
            TextOptions[] TextOptions)
        {
            this.TextFonts = TextFonts;
            this.Offsets = Offsets;
            this.Texts = Texts;
            this.TextColors = TextColors;
            this.TextOptions = TextOptions;
        }

        public string[] Texts { get; }
        public int[] Offsets { get; }
        public Font[] TextFonts { get; }
        public Color[] TextColors { get; }
        public TextOptions[] TextOptions { get; }
    }
}