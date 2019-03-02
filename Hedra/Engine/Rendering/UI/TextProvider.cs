using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using OpenTK;

namespace Hedra.Engine.Rendering.UI
{
    public class TextProvider : ITextProvider
    {
        private static readonly Dictionary<string, Color> ColorMap;
        private static readonly Dictionary<string, Font> FontMap;
        private static readonly Dictionary<string, float> SizeMap;
        private static readonly Dictionary<string, Func<string, string>> TransformationMap;

        static TextProvider()
        {
            ColorMap = new Dictionary<string, Color>
            {
                {TextFormatting.Red, Color.Red},
                {TextFormatting.Violet, Color.MediumVioletRed},
                {TextFormatting.Blue, Color.CornflowerBlue},
                {TextFormatting.White, Color.White},
                {TextFormatting.Green, Color.LawnGreen},
                {TextFormatting.Orange, Color.OrangeRed},
                {TextFormatting.Gold, Color.Gold},
                {TextFormatting.Gray, Color.LightGray},
                {TextFormatting.Black, Color.Black},
                {TextFormatting.Yellow, Color.Yellow},
                {TextFormatting.Cyan, Color.Cyan}
            };
            FontMap = new Dictionary<string, Font>
            {
                {TextFormatting.Bold, FontCache.Get(AssetManager.BoldFamily, 1, FontStyle.Bold)},
                {TextFormatting.Normal, FontCache.Get(AssetManager.NormalFamily, 1)}
            };
            SizeMap = new Dictionary<string, float>
            {
                {TextFormatting.Smaller, .8f},
                {TextFormatting.Bigger, 1.25f},
            };
            TransformationMap = new Dictionary<string, Func<string, string>>
            {
                {TextFormatting.Caps, S => S.ToUpperInvariant()},
            };
        }
        
        public Bitmap BuildText(string Text, Font TextFont, Color TextColor, TextOptions Options)
        {
            return DoBuildText(BuildParams(Text, TextFont, TextColor, Options));
        }
        
        public Bitmap BuildText(string Text, Font TextFont, Color TextColor)
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
                            newSplit += $"{splits[i].Replace(texts[i], subParts[k])}{Environment.NewLine}";
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
                if(accumulated + texts[i].Length >= End) break;
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

        private static void Match(string Text, Color Default, Font DefaultFont, out Color Color, out string CleanVersion, out Font Font)
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
                    F => FontCache.Get(F.FontFamily, DefaultFont.Size, F.Style)
                );
                var lambdaFont = Font;
                Font = Replace(
                    ref Text,
                    Font,
                    SizeMap,
                    U => FontCache.Get(lambdaFont.FontFamily, lambdaFont.Size * U, lambdaFont.Style)
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
            Match(Text, Color.Empty, TextFont, out _, out _, out var font);
            return font;
        }
        
        private static Color ColorMatch(string Text, Color Default)
        {
            Match(Text, Default, FontCache.Default, out var color, out _, out _);
            return color;
        }
        
        private static string StringMatch(string Text)
        {
            Match(Text, Color.Empty, FontCache.Default, out _, out var cleaned, out _);
            return cleaned;
        }
        
        private static Bitmap DoBuildText(TextParams Params)
        {
            if(Params.TextFonts.Any(F => F.Size > 128) || Params.Texts.Length == 0) return new Bitmap(1,1);
            var fullString = string.Join(string.Empty, Params.Texts);
            var size = CalculateNeededSize(Params);
            var textBitmap = new Bitmap((int)Math.Ceiling(Math.Max(size.Width, 1)), (int)Math.Ceiling(Math.Max(size.Height,1)));
            using (var graphics = Graphics.FromImage(textBitmap))
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                if (OSManager.RunningPlatform == Platform.Windows)
                {
                    // Mono doesnt support this quite well.
                    graphics.ScaleTransform(1.3f, 1.3f);
                }
                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                var offset = PointF.Empty;
                var bounds = RectangleF.Empty;
                for (var i = 0; i < Params.Texts.Length; ++i)
                {
                    if (Params.Texts[i] == Environment.NewLine)
                    {
                        offset = new PointF(0, offset.Y + bounds.Height * .75f);
                        continue;
                    }
                        
                    /* Draw shadows & strokes */
                    if (Params.TextOptions[i].HasStroke)
                        AddStroke(graphics, Params.Texts[i], Params.TextFonts[i], offset, Params.TextOptions[i]);
                    else
                        AddShadows(graphics, Params.Texts[i], Params.TextFonts[i], offset);
                    
                    using (var brush = new SolidBrush(Params.TextColors[i]))
                    {
                        using (var gp = new GraphicsPath())
                        {
                            gp.AddString(
                                Params.Texts[i],
                                Params.TextFonts[i].FontFamily,
                                (int) Params.TextFonts[i].Style,
                                Params.TextFonts[i].Size,
                                Point.Empty,
                                StringFormat.GenericTypographic
                            );
                            var offsetMat = new Matrix();
                            offsetMat.Translate(offset.X, offset.Y);
                            gp.Transform(offsetMat);
                            graphics.FillPath(brush, gp);
                            var format = (StringFormat) StringFormat.GenericTypographic.Clone();
                            format.SetMeasurableCharacterRanges(new[]
                            {
                                new CharacterRange(Params.Offsets[i], Params.Texts[i].Length)
                            });
                            var region = graphics.MeasureCharacterRanges(
                                fullString,
                                Params.TextFonts[i],
                                new RectangleF(0, 0, 0, 0),
                                format
                            ).First();
                            bounds = region.GetBounds(graphics);
                            offset = new PointF(
                                bounds.Width * .75f + offset.X,
                                offset.Y
                            );
                        }
                    }
                }
            }
            return textBitmap;
        }

        private static void AddStroke(Graphics Graphics, string Text, Font TextFont, PointF Offset, TextOptions Options)
        {
            AddBackground(Graphics, Text, TextFont, Color.FromArgb(80, Options.StrokeColor.R, Options.StrokeColor.G, Options.StrokeColor.B), new Vector2(Offset.X, Offset.Y + 1.75f));
            AddBackground(Graphics, Text, TextFont, Color.FromArgb(80, Options.StrokeColor.R, Options.StrokeColor.G, Options.StrokeColor.B), new Vector2(Offset.X + 1.75f, Offset.Y));
            AddBackground(Graphics, Text, TextFont, Color.FromArgb(80, Options.StrokeColor.R, Options.StrokeColor.G, Options.StrokeColor.B), new Vector2(Offset.X, Offset.Y - 1.75f));
            AddBackground(Graphics, Text, TextFont, Color.FromArgb(80, Options.StrokeColor.R, Options.StrokeColor.G, Options.StrokeColor.B), new Vector2(Offset.X - 1.75f, Offset.Y));
        }

        private static void AddShadows(Graphics Graphics, string Text, Font TextFont, PointF Offset)
        {
            AddBackground(Graphics, Text, TextFont, Color.FromArgb(80, 0, 0, 0), new Vector2(Offset.X + 1.5f, Offset.Y + 1.5f));
        }

        private static void AddBackground(Graphics Graphics, string Text, Font TextFont, Color TextColor, Vector2 Offset)
        {
            using (var gp = new GraphicsPath())
            {
                using (var shadowBrush = new SolidBrush(TextColor))
                {
                    gp.AddString(
                        Text,
                        TextFont.FontFamily,
                        (int) TextFont.Style,
                        TextFont.Size,
                        Point.Empty, StringFormat.GenericTypographic
                    );
                    var shadowOffset = new Matrix();
                    shadowOffset.Translate(Offset.X, Offset.Y);
                    gp.Transform(shadowOffset);

                    Graphics.FillPath(shadowBrush, gp);
                }
            }
        }
        
        private static SizeF CalculateNeededSize(TextParams Params)
        {
            var max = SizeF.Empty;
            var sizes = new SizeF[Params.Texts.Length];
            var fullString = string.Join(string.Empty, Params.Texts);
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                for (var i = 0; i < Params.Texts.Length; ++i)
                {
                    if (Params.Texts[i] == Environment.NewLine) continue;
                    var format = StringFormat.GenericTypographic;
                    format.SetMeasurableCharacterRanges(new[]
                    {
                        new CharacterRange(Params.Offsets[i], Params.Texts[i].Length)
                    });
                    var rectangle = graphics.MeasureCharacterRanges(
                        fullString,
                        Params.TextFonts[i],
                        new RectangleF(0, 0, 0, 0),
                        format
                    ).First().GetBounds(graphics);
                    sizes[i] = new SizeF(rectangle.Width, rectangle.Height);
                }
            }
            var bounds = SizeF.Empty;
            var offset = SizeF.Empty;
            for (var i = 0; i < sizes.Length; ++i)
            {
                if (Params.Texts[i] == Environment.NewLine && i != sizes.Length-1)
                {
                    max = new SizeF(max.Width, max.Height + bounds.Height);
                    offset = SizeF.Empty;
                }
                else
                {
                    bounds = sizes[i];
                    offset += bounds;
                    max = new SizeF(Math.Max(offset.Width, max.Width), Math.Max(max.Height, bounds.Height));
                }   
            }
            return max;
        }
        
        private static SizeF CalculateTextSize(string Text, Font TextFont)
        {
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                return graphics.MeasureString(Text, TextFont);
            }
        }
    }

    public class TextParams
    {
        public string[] Texts { get; }
        public int[] Offsets { get; }
        public Font[] TextFonts { get; }
        public Color[] TextColors { get; }
        public TextOptions[] TextOptions { get; }

        public TextParams(string[] Texts, int[] Offsets, Font[] TextFonts, Color[] TextColors, TextOptions[] TextOptions)
        {
            this.TextFonts = TextFonts;
            this.Offsets = Offsets;
            this.Texts = Texts;
            this.TextColors = TextColors;
            this.TextOptions = TextOptions;
        }
    }
}