using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    public class TextProvider : ITextProvider
    {
        public Bitmap BuildText(string Text, Font TextFont, Color TextColor)
        {
            if(TextFont.Size > 128) return new Bitmap(1,1);
            var size = CalculateTextSize(Text, TextFont);
            var textBitmap = new Bitmap((int)Math.Ceiling(Math.Max(size.Width, 1)), (int)Math.Ceiling(Math.Max(size.Height,1)));
            using (var graphics = Graphics.FromImage(textBitmap))
            {
                if (OSManager.RunningPlatform == Platform.Windows)
                {
                    // Mono doesnt support this quite well.
                    graphics.ScaleTransform(1.3f, 1.3f);
                }

                // Draw shadows
                using (var gp = new GraphicsPath())
                {
                    using (Brush shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                    {
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                        gp.AddString(Text, TextFont.FontFamily, (int) TextFont.Style, TextFont.Size,
                            Point.Empty, StringFormat.GenericTypographic);
                        var shadowOffset = new Matrix();
                        shadowOffset.Translate(1, 1);
                        gp.Transform(shadowOffset);

                        graphics.FillPath(shadowBrush, gp);
                    }
                }
                // Draw text
                using (var brush = new SolidBrush(TextColor))
                {
                    using (var gp = new GraphicsPath())
                    {
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        gp.AddString(Text, TextFont.FontFamily, (int) TextFont.Style, TextFont.Size,
                            Point.Empty, StringFormat.GenericTypographic);
                        graphics.FillPath(brush, gp);
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
}