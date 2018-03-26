/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 08:29 p.m.
 *
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of GUIText.
    /// </summary>
    public class GUIText : UIElement, IDisposable
    {
        public static Vector2 DefaultSize = new Vector2(GameSettings.Width, GameSettings.Height);
        public GUITexture UIText { get; private set; }
        public TextConfiguration Configuration { get; set; }
        public SizeF Size { get; private set; }
        public AlignMode Align = AlignMode.Center;
        private Vector2 _temporalPosition;
        private string _text;

        public GUIText(string Text, Vector2 Position, Color TextColor, Font TextFont)
        {
            this._text = Text;
            this._temporalPosition = Position;
            this.Configuration = new TextConfiguration(TextColor, TextFont);
            this.MakeText();
        }

        public void MakeText()
        {
            this.CalculateTextSize(_text);
            var textBitmap = new Bitmap((int)Math.Ceiling(Math.Max(Size.Width, 1)), (int)Math.Ceiling(Math.Max(Size.Height,1)));
            using (var graphics = Graphics.FromImage(textBitmap))
            {
                graphics.ScaleTransform(1.3f, 1.3f);
                #region Draw Shadows
                using (var gp = new GraphicsPath())
                {
                    using (Brush shadowBrush = new SolidBrush(System.Drawing.Color.FromArgb(80, 0, 0, 0)))
                    {
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));
                        gp.AddString(Text, TextFont.FontFamily, (int) TextFont.Style, TextFont.Size,
                            Point.Empty, StringFormat.GenericTypographic);
                        var shadowOffset = new Matrix();
                        shadowOffset.Translate(1, 1);
                        gp.Transform(shadowOffset);

                        graphics.FillPath(shadowBrush, gp);
                    }
                }
                #endregion

                using(var brush = new SolidBrush(TextColor))
                using (var gp = new GraphicsPath())
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    gp.AddString(Text, TextFont.FontFamily, (int)TextFont.Style, TextFont.Size,
                        Point.Empty, StringFormat.GenericTypographic);
                    graphics.FillPath(brush, gp);
                }
            }
            
            var previousState = UIText?.IsEnabled ?? false;
            DrawManager.UIRenderer.Remove(UIText);
            UIText?.Dispose();

            UIText = new GUITexture(Graphics2D.LoadTexture(textBitmap),
                new Vector2(Size.Width / DefaultSize.X, Size.Height / DefaultSize.Y), _temporalPosition);
            DrawManager.UIRenderer.Add(UIText);

            if (Align == AlignMode.Left)
            {
                UIText.Position -= UIText.Scale;
            }
            UIText.IsEnabled = previousState;
        }

        private void CalculateTextSize(string FullText)
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                Size = graphics.MeasureString(FullText, TextFont);
            }
        }

        public void Update()
        {
            this.MakeText();
        }

        public Color TextColor
        {
            get { return Configuration.Color; }
            set { Configuration.Color = value; }
        }

        public Font TextFont
        {
            get { return Configuration.Font; }
            set { Configuration.Font = value; }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text || Configuration == null || _text == null) return;
                _text = value;
                this.MakeText();
            }
        }

        public Vector4 Color
        {
            get { return this.UIText.Color; }
            set { this.UIText.Color = value; }
        }

        public Vector2 Scale
        {
            get
            {
                if (UIText != null)
                    return UIText.Scale;
                else
                    return new Vector2(1, 1);
            }
            set
            {
                if (UIText != null)
                    UIText.Scale = value;
            }
        }

        public Vector2 Position
        {
            get { return UIText.Position; }
            set
            {
                UIText.Position = value;
                _temporalPosition = value;
            }
        }

        public void Enable()
        {
            UIText.IsEnabled = true;
        }

        public void Disable()
        {
            UIText.IsEnabled = false;
        }

        ~GUIText()
        {
            ThreadManager.ExecuteOnMainThread(this.Dispose);
        }

        public void Dispose()
        {
            DrawManager.UIRenderer.Remove(UIText);
        }
    }

    public enum AlignMode
    {
        Left,
        Center
    }
}