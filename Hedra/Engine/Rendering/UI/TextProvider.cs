using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;
using Hedra.Engine.Native;

namespace Hedra.Engine.Rendering.UI
{
    public class TextProvider : ITextProvider
    {
        private static readonly Dictionary<string, Color> ColorMap;

        static TextProvider()
        {
            ColorMap = new Dictionary<string, Color>
            {
                {DisplayColor.Red, Color.Red},
                {DisplayColor.Violet, Color.MediumVioletRed},
                {DisplayColor.Blue, Color.CornflowerBlue},
                {DisplayColor.White, Color.White},
                {DisplayColor.Green, Color.LawnGreen},
                {DisplayColor.Orange, Color.OrangeRed},
            };
        }
        
        public Bitmap BuildText(string Text, Font TextFont, Color TextColor)
        {
            return DoBuildText(BuildParams(Text, TextFont, TextColor));
        }

        public static TextParams BuildParams(string Text, Font TextFont, Color TextColor)
        {
            var splits = Regex.Split(Text, @"(\$.+?\b)").Where( S => !string.IsNullOrWhiteSpace(S)).ToArray();
            return new TextParams(
                Text,
                splits.Select(StringMatch).ToArray(),
                splits.Select(S => Text.IndexOf(S, StringComparison.Ordinal)).ToArray(),
                TextFont,
                splits.Select(S => ColorMatch(S, TextColor)).ToArray()
            );
        }

        private static bool Match(string Text, Color Default, out Color Color, out string CleanVersion)
        {
            CleanVersion = Text;
            foreach (var pair in ColorMap)
            {
                var regex = $@"\{pair.Key}";
                if (!Regex.IsMatch(Text, regex)) continue;
                Color = pair.Value;
                CleanVersion = Regex.Replace(Text, regex, string.Empty);
                return true;
            }
            Color = Default;
            return false;
        }

        private static Color ColorMatch(string Text, Color Default)
        {
            Match(Text, Default, out var color, out _);
            return color;
        }
        
        private static string StringMatch(string Text)
        {
            Match(Text, Color.Empty, out _, out var cleaned);
            return cleaned;
        }
        
        private static Bitmap DoBuildText(TextParams Params)
        {
            if(Params.TextFont.Size > 128 || Params.Texts.Length == 0) return new Bitmap(1,1);
            var fullString = string.Join(string.Empty, Params.Texts);
            var size = CalculateTextSize(fullString, Params.TextFont);
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
                for (var i = 0; i < Params.Texts.Length; ++i)
                {
                    // Draw shadows
                    using (var gp = new GraphicsPath())
                    {
                        using (var shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                        {
                            gp.AddString(Params.Texts[i], Params.TextFont.FontFamily, (int) Params.TextFont.Style,
                                Params.TextFont.Size,
                                Point.Empty, StringFormat.GenericTypographic);
                            var shadowOffset = new Matrix();
                            shadowOffset.Translate(1 + offset.X, 1 + offset.Y);
                            gp.Transform(shadowOffset);

                            graphics.FillPath(shadowBrush, gp);
                        }
                    }
                
                    using (var brush = new SolidBrush(Params.TextColors[i]))
                    {
                        using (var gp = new GraphicsPath())
                        {
                            gp.AddString(
                                Params.Texts[i],
                                Params.TextFont.FontFamily,
                                (int) Params.TextFont.Style,
                                Params.TextFont.Size,
                                Point.Empty,
                                StringFormat.GenericTypographic
                            );
                            var offsetMat = new Matrix();
                            offsetMat.Translate(offset.X, offset.Y);
                            gp.Transform(offsetMat);
                            graphics.FillPath(brush, gp);
                            var format = StringFormat.GenericTypographic;
                            format.SetMeasurableCharacterRanges(new[]
                            {
                                new CharacterRange(Params.Offsets[i], Params.Texts[i].Length)
                            });
                            var region = graphics.MeasureCharacterRanges(
                                Params.WholeText,
                                Params.TextFont,
                                new RectangleF(0, 0, 0, 0),
                                format
                            ).First();
                            var bounds = region.GetBounds(graphics);
                            offset = new PointF(bounds.Width * .8f + offset.X, bounds.Height * 0f + offset.Y);
                        }
                    }
                }
            }
            return textBitmap;
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
        public string WholeText { get; }
        public string[] Texts { get; }
        public int[] Offsets { get; }
        public Font TextFont { get; }
        public Color[] TextColors { get; }

        public TextParams(string WholeText, string[] Texts, int[] Offsets, Font TextFont, Color[] TextColors)
        {
            this.WholeText = WholeText;
            this.TextFont = TextFont;
            this.Offsets = Offsets;
            this.Texts = Texts;
            this.TextColors = TextColors;
        }
    }
}