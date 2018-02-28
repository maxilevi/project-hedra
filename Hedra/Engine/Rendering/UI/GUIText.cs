/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 08:29 p.m.
 *
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of GUIText.
    /// </summary>
    public class GUIText : UIElement, IDisposable
    {
        public GUITexture UiText;
        public SizeF Size { get; private set; }
        public Color FontColor;
        public Font TextFont;
        private Vector2 _temporalPosition;
        private Vector2 _temporalScale;
        public AlignMode Align = AlignMode.Center;
        public static Vector2 DefaultSize = new Vector2(Constants.WIDTH, Constants.HEIGHT);

        public GUIText(string Text, Vector2 Position, Color FontColor, System.Drawing.Font TextFont)
        {
            this._text = Text;
            this.TextFont = TextFont;
            this.FontColor = FontColor;
            this._temporalPosition = Position;
            this._temporalScale = Scale;
            this.MakeText();
        }

        public void MakeText()
        {
            Bitmap textBitmap = new Bitmap(1, 1);
            SolidBrush brush = new SolidBrush(FontColor);
            using (Graphics graphics = Graphics.FromImage(textBitmap))
            {
                Size = graphics.MeasureString(Text, TextFont);
                if (Size.Width != 0 && Size.Height != 0)
                    textBitmap = new Bitmap(textBitmap, (int) Math.Ceiling(Size.Width),
                        (int) Math.Ceiling(Size.Height));
            }


            using (Graphics graphics = Graphics.FromImage(textBitmap))
            {
                StringFormat sf = new StringFormat();
                graphics.ScaleTransform(1.3f, 1.3f);
                using (GraphicsPath gp = new GraphicsPath())
                {
                    using (Brush ShadowBrush = new SolidBrush(System.Drawing.Color.FromArgb(80, 0, 0, 0)))
                    {
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        graphics.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));
                        gp.AddString(Text, TextFont.FontFamily, (int) TextFont.Style, TextFont.Size,
                            new RectangleF(PointF.Empty, Size), sf);
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        Matrix ShadowOffset = new Matrix();
                        ShadowOffset.Translate(1, 1);
                        gp.Transform(ShadowOffset);
                        graphics.FillPath(ShadowBrush, gp);
                    }
                }

                Pen outlinePen = new Pen(System.Drawing.Color.FromArgb(255, 39, 39, 39), 2.00f);

                using (GraphicsPath gp = new GraphicsPath())
                {
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    gp.AddString(Text, TextFont.FontFamily, (int) TextFont.Style, TextFont.Size,
                        new RectangleF(PointF.Empty, Size), sf);
                    graphics.SmoothingMode = SmoothingMode.HighQuality;

                    graphics.FillPath(brush, gp);
                }

                outlinePen.Dispose();
                sf.Dispose();
            }


            uint newId;
            newId = Graphics2D.LoadTexture(textBitmap);
            //else newId = Graphics2D.LoadTexture( Graphics2D.ReColorMask(FontColor, textBitmap) );
            GUITexture uiText2 = new GUITexture(newId,
                new Vector2(Size.Width / DefaultSize.X, Size.Height / DefaultSize.Y), _temporalPosition);
            uiText2.IsEnabled = true;
            DrawManager.UIRenderer.Add(uiText2);
            DrawManager.UIRenderer.Remove(UiText);
            UiText?.Dispose();
            UiText = uiText2;
            if (Align == AlignMode.Left)
            {
                UiText.Position -= UiText.Scale;
            }
            textBitmap.Dispose();
            brush.Dispose();
        }

        public void Update()
        {
            MakeText();
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (value != _text && TextFont != null && _text != null)
                {
                    _text = value;
                    MakeText();
                }
            }
        }

        public Vector4 Color
        {
            get { return this.UiText.Color; }
            set { this.UiText.Color = value; }
        }

        public Vector2 Scale
        {
            get
            {
                if (UiText != null)
                    return UiText.Scale;
                else
                    return new Vector2(1, 1);
            }
            set
            {
                if (UiText != null)
                    UiText.Scale = value;
            }
        }

        public Vector2 Position
        {
            get { return UiText.Position; }
            set { UiText.Position = value; }
        }

        public void Enable()
        {
            UiText.IsEnabled = true;
        }

        public void Disable()
        {
            UiText.IsEnabled = false;
        }

        ~GUIText()
        {
            ThreadManager.ExecuteOnMainThread(this.Dispose);
        }

        public void Dispose()
        {
            DrawManager.UIRenderer.Remove(UiText);
        }
    }

    public enum AlignMode
    {
        Left,
        Center
    }
}