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
    public class GUIText : UIElement, ISimpleTexture
    {
        public static ITextProvider Provider { get; set; } = new TextProvider();
        public GUITexture UIText { get; private set; }
        private AlignMode _align = AlignMode.Center;
        private static readonly Vector2 DefaultSize = new Vector2(GameSettings.Width, GameSettings.Height);
        private readonly TextConfiguration _configuration;
        private Vector2 _temporalPosition;
        private string _text;

        public GUIText(string Text, Vector2 Position, Color TextColor, Font TextFont)
        {
            _text = Text;
            _temporalPosition = Position;
            _configuration = new TextConfiguration(TextColor, TextFont);
            this.UpdateText();
        }

        public void UpdateText()
        {
            var textBitmap = Provider.BuildText(Text, TextFont, TextColor);
            var previousState = UIText?.Enabled ?? false;
            DrawManager.UIRenderer.Remove(UIText);
            UIText?.Dispose();
            var size = new Vector2(textBitmap.Width, textBitmap.Height);
            UIText = new GUITexture(Graphics2D.LoadTexture(textBitmap),
                new Vector2(size.X / DefaultSize.X, size.Y / DefaultSize.Y), _temporalPosition);
            DrawManager.UIRenderer.Add(UIText);
            
            if (_align == AlignMode.Left)
            {
                UIText.Position -= UIText.Scale;
            }
            UIText.Enabled = previousState;
        }

        public Color TextColor
        {
            get => _configuration.Color;
            set => _configuration.Color = value;
        }

        public Font TextFont
        {
            get => _configuration.Font;
            set => _configuration.Font = value;
        }

        public string Text
        {
            get => _text;
            set
            {
                if (value == _text || _configuration == null || _text == null) return;
                _text = value;
                this.UpdateText();
            }
        }

        public Vector2 Scale
        {
            get => UIText?.Scale ?? Vector2.Zero;
            set
            {
                if (UIText != null)
                    UIText.Scale = value;
            }
        }

        public Vector2 Position
        {
            get => UIText.Position;
            set
            {
                UIText.Position = value;
                _temporalPosition = value;
            }
        }

        public void Enable()
        {
            UIText.Enabled = true;
        }

        public void Disable()
        {
            UIText.Enabled = false;
        }

        public void Dispose()
        {
            UIText?.Dispose();
            DrawManager.UIRenderer.Remove(UIText);
        }
    }

    public enum AlignMode
    {
        Left,
        Center
    }
}