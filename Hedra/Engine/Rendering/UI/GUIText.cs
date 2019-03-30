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
using System.Threading;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of GUIText.
    /// </summary>
    public class GUIText : UIElement, ISimpleTexture, ITransparent
    {
        public static ITextProvider Provider { get; set; } = new TextProvider();
        public GUITexture UIText { get; private set; }
        private AlignMode _align = AlignMode.Center;
        private static readonly Vector2 DefaultSize = new Vector2(GameSettings.Width, GameSettings.Height);
        private readonly TextConfiguration _configuration;
        private Vector2 _temporalPosition;
        private string _text;
        private Translation _translation;
        
        public GUIText(Translation Translation, Vector2 Position, Color TextColor, Font TextFont)
        {
            _temporalPosition = Position;
            _configuration = new TextConfiguration(TextColor, TextFont);
            SetTranslation(Translation);
        }

        public GUIText(string Text, Vector2 Position, Color TextColor, Font TextFont) 
            : this(Translation.Default(Text), Position, TextColor, TextFont)
        {         
        }

        private static BitmapObject BuildBitmap(string Text, Color Color, Font Font, TextOptions Options, out Vector2 Measurements)
        {
            const float crispModifier = 1.5f;
            var textBitmap = Provider.BuildText(Text, FontCache.Get(Font, Font.Size * crispModifier), Color, Options);
            Measurements = 
                new Vector2((float) (textBitmap.Width * (1.0 / crispModifier)), (float) (textBitmap.Height * (1.0 / crispModifier)));
            var obj = new BitmapObject
            {
                Bitmap = textBitmap,
                Path = $"Text:{Text}"
            };
            return obj;
        }

        public static uint BuildText(string Text, Color Color, Font Font, out Vector2 Measurements)
        {
            return Graphics2D.LoadTexture(BuildBitmap(Text, Color, Font, new TextOptions(), out Measurements));
        }
        
        public void UpdateText()
        {
            var obj = BuildBitmap(Text, TextColor, TextFont, BuildTextOptions(), out var measurements);
            var previousState = UIText?.Enabled ?? false;
            DrawManager.UIRenderer.Remove(UIText);
            UIText?.Dispose();
            UIText = new GUITexture(GUIRenderer.TransparentTexture,
                new Vector2(measurements.X / DefaultSize.X, measurements.Y / DefaultSize.Y), _temporalPosition);
            DrawManager.UIRenderer.Add(UIText);

            if (_align == AlignMode.Left)
            {
                UIText.Position -= UIText.Scale;
            }

            UIText.Enabled = previousState;
            if (Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                Executer.ExecuteOnMainThread(() => UIText.TextureId = Graphics2D.LoadTexture(obj));
            else
                UIText.TextureId = Graphics2D.LoadTexture(obj);        
        }

        private TextOptions BuildTextOptions()
        {
            return new TextOptions
            {
                HasStroke = Stroke,
                StrokeColor = StrokeColor,
                StrokeWidth = StrokeWidth
            };
        }

        public void SetTranslation(Translation Translation)
        {
            _translation?.Dispose();
            _translation = Translation;
            Translation.LanguageChanged += delegate
            {
                Text = Translation.Get();
            };
            Text = Translation.Get();
        }
        
        public Color TextColor
        {
            get => _configuration.Color;
            set
            {
                if (_configuration.Color == value) return;
                _configuration.Color = value;
                this.UpdateText();
            }
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
                if (value == _text || _configuration == null || value == null) return;
                _text = value;
                this.UpdateText();
            }
        }

        public Vector2 Scale
        {
            get => UIText.Scale;
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

        public float Opacity
        {
            get => UIText.Opacity;
            set => UIText.Opacity = value;
        }

        public bool Grayscale
        {
            get => UIText.Grayscale;
            set => UIText.Grayscale = value;
        }
        
        public bool Stroke { get; set; }
        
        public float StrokeWidth { get; set; }
        
        public Color StrokeColor { get; set; }

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