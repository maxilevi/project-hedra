/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 08:29 p.m.
 *
 */

using System.Numerics;
using System.Threading;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Rendering.UI
{
    /// <summary>
    ///     Description of GUIText.
    /// </summary>
    public class GUIText : UIElement, ISimpleTexture, ITransparent
    {
        private static readonly Vector2 DefaultSize = new Vector2(GameSettings.Width, GameSettings.Height);
        private const float CrispModifier = 1.5f;
        private readonly object _updateLock = new object();
        private bool _disposed;
        private int _updateVersion;
        private bool _stroke;
        private float _strokeWidth = 1.75f;
        private Color _strokeColor = Color.Black;
        private readonly TextConfiguration _configuration;
        private readonly AlignMode _align = AlignMode.Center;
        private Vector2 _temporalPosition;
        private string _text;
        private Translation _translation;

        public GUIText(Translation Translation, Vector2 Position, Color TextColor, Font TextFont)
        {
            _temporalPosition = Position;
            _configuration = new TextConfiguration(TextColor, TextFont);
            UIText = new GUITexture(0, Vector2.Zero, Position) { UseTextureCache = false };
            DrawManager.UIRenderer.Add(UIText);
            SetTranslation(Translation);
        }

        public GUIText(string Text, Vector2 Position, Color TextColor, Font TextFont)
            : this(Translation.Default(Text), Position, TextColor, TextFont)
        {
        }

        public static ITextProvider Provider { get; set; } = new TextProvider();
        public GUITexture UIText { get; private set; }

        public Color TextColor
        {
            get => _configuration.Color;
            set
            {
                if (_configuration.Color == value) return;
                _configuration.Color = value;
                UpdateText();
            }
        }

        public Font TextFont
        {
            get => _configuration.Font;
            set
            {
                if (Equals(_configuration.Font, value)) return;
                _configuration.Font = value;
                UpdateText();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                if (value == _text || _configuration == null || value == null) return;
                _text = value;
                UpdateText();
            }
        }

        public bool Grayscale
        {
            get => UIText.Grayscale;
            set => UIText.Grayscale = value;
        }

        public bool Stroke
        {
            get => _stroke;
            set { if (_stroke == value) return; _stroke = value; UpdateText(); }
        }

        public float StrokeWidth
        {
            get => _strokeWidth;
            set { if (_strokeWidth == value) return; _strokeWidth = value; UpdateText(); }
        }

        public Color StrokeColor
        {
            get => _strokeColor;
            set { if (_strokeColor == value) return; _strokeColor = value; UpdateText(); }
        }

        public bool Enabled => UIText.Enabled;

        public float Opacity
        {
            get => UIText.Opacity;
            set => UIText.Opacity = value;
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
            lock (_updateLock)
            {
                if (_disposed) return;
                _disposed = true;
                ++_updateVersion;
                _translation?.Dispose();
                TextCache.Remove(UIText.TextureId);
                UIText.TextureId = 0;
                UIText.Dispose();
                RemoveFromRenderer();
            }
        }

        public void RemoveFromRenderer()
        {
            DrawManager.UIRenderer.Remove(UIText);
        }

        private static BitmapObject BuildBitmap(string Text, Color Color, Font Font, TextOptions Options,
            out Vector2 Measurements)
        {
            var textBitmap = Provider.BuildText(Text, FontCache.Get(Font, Font.Size * CrispModifier), Color, Options);
            Measurements =
                new Vector2((float)(textBitmap.Width * (1.0 / CrispModifier)),
                    (float)(textBitmap.Height * (1.0 / CrispModifier)));
            var obj = new BitmapObject
            {
                Bitmap = textBitmap,
                Path = $"Text:{Text}"
            };
            return obj;
        }

        public static BitmapObject BuildText(string Text, Color Color, Font Font, out Vector2 Measurements)
        {
            return BuildBitmap(Text, Color, Font, new TextOptions(), out Measurements);
        }

        public static uint LoadText(BitmapObject Bmp)
        {
            return Graphics2D.LoadTexture(Bmp);
        }

        public void UpdateText()
        {
            lock (_updateLock)
            {
                if (_disposed) return;
                var version = ++_updateVersion;
                var text = Text;
                var font = TextFont;
                var color = TextColor;
                var options = BuildTextOptions();
                var rasterScale = TextProvider.RenderingScale;

                if (text == string.Empty)
                {
                    SetTexture(0, Vector2.Zero);
                    return;
                }

                // Cache hits do not need a bitmap, measurement, or render-thread upload.
                if (TextCache.TryUse(text, font, color, options, rasterScale, out var cachedId, out var cachedSize))
                {
                    SetTexture(cachedId, cachedSize);
                    return;
                }

                var bitmap = BuildBitmap(text, color, font, options, out var measurements);
                UIText.Scale = measurements / DefaultSize;
                void UpdateTexture()
                {
                    try
                    {
                        lock (_updateLock)
                        {
                            if (_disposed || version != _updateVersion) return;
                            var id = TextCache.UseOrCreate(text, font, color, options, rasterScale, bitmap,
                                out var pixelSize);
                            SetTexture(id, pixelSize);
                        }
                    }
                    finally
                    {
                        bitmap.Bitmap.Dispose();
                    }
                }

                if (Thread.CurrentThread.ManagedThreadId != Engine.Loader.Hedra.RenderingThreadId)
                    Executer.ExecuteOnMainThread(UpdateTexture);
                else
                    UpdateTexture();
            }
        }

        private void SetTexture(uint Id, Vector2 PixelSize)
        {
            var previous = UIText.TextureId;
            UIText.TextureId = Id;
            UIText.Scale = PixelSize / CrispModifier / DefaultSize;
            if (_align == AlignMode.Left) UIText.Position = _temporalPosition - UIText.Scale;
            TextCache.Remove(previous);
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
            Translation.LanguageChanged += delegate { Text = Translation.Get(); };
            Text = Translation.Get();
        }
    }

    public enum AlignMode
    {
        Left,
        Center
    }
}