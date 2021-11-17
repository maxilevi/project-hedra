/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 12:54 a.m.
 *
 */

using System.Numerics;
using Hedra.Engine.Events;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering.UI;
using Hedra.Sound;
using Silk.NET.Input;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public delegate void OnButtonClickEventHandler(object Sender, MouseButtonEventArgs E);


    public delegate void OnButtonHoverEnterEventHandler();

    public delegate void OnButtonHoverExitEventHandler();

    public class Button : EventListener, UIElement
    {
        private bool _hasEntered;
        private Translation _liveTranslation;
        private Vector2 _position;
        private Color _previousFontColor;
        private Vector2 _previousScale;
        private GUIText _privateText;

        public Button(Vector2 Position, Vector2 Scale, Translation Translation, Color FontColor, Font TextFont)
        {
            Initialize(Position, Scale, Translation.Get(), Translation, 0, FontColor, TextFont);
        }

        public Button(Vector2 Position, Vector2 Scale, string Text, Color FontColor, Font TextFont)
        {
            Initialize(Position, Scale, Text, null, 0, FontColor, TextFont);
        }

        public Button(Vector2 Position, Vector2 Scale, uint Texture)
        {
            Initialize(Position, Scale, null, null, Texture, Color.Black, FontCache.Default);
        }

        public bool CanClick { get; set; } = true;
        public bool PlaySound { get; set; } = true;
        public GUITexture Texture { get; set; }

        public bool Enabled { get; private set; }

        public GUIText Text
        {
            get => _privateText;
            private set
            {
                _privateText?.Dispose();
                _privateText = value;
            }
        }

        public void Disable()
        {
            Enabled = false;
            Text?.Disable();
            if (Texture != null)
                Texture.Enabled = false;
        }

        public void Enable()
        {
            Enabled = true;
            Text?.Enable();
            if (Texture != null)
                Texture.Enabled = true;
        }

        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                if (Text != null)
                    Text.Position = value;
                if (Texture != null)
                    Texture.Position = value;
            }
        }

        public Vector2 Scale
        {
            get => Text?.Scale ?? Texture.Scale;
            set
            {
                if (Text != null)
                    Text.Scale = value;
                if (Texture != null)
                    Texture.Scale = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Text?.Dispose();
            if (Texture != null)
            {
                Texture.Dispose();
                DrawManager.UIRenderer.Remove(Texture);
            }
        }

        public event OnButtonClickEventHandler Click;
        public event OnButtonHoverEnterEventHandler HoverEnter;
        public event OnButtonHoverExitEventHandler HoverExit;

        private void Initialize(Vector2 Position, Vector2 Scale, string Text, Translation Translation, uint TextureId,
            Color FontColor, Font F)
        {
            if (TextureId != 0)
                Texture = new GUITexture(TextureId, Scale, Position);
            if (Texture != null)
                DrawManager.UIRenderer.Add(Texture);

            if (!string.IsNullOrEmpty(Text) || Translation != null)
            {
                _liveTranslation = Translation ?? Translation.Default(Text);
                this.Text = new GUIText(_liveTranslation, Position, FontColor, F);
            }

            this.Position = new Vector2(Position.X, Position.Y);

            HoverEnter += OnHoverEnter;
            HoverExit += OnHoverExit;
        }

        public void ForceClick()
        {
            Click?.Invoke(null, default);
        }

        public override void OnMouseButtonDown(object Sender, MouseButtonEventArgs E)
        {
            if (Enabled && Click != null && (E.Button == MouseButton.Left || E.Button == MouseButton.Right))
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(
                    new Vector2(E.Position.X, E.Position.Y),
                    new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                );
                if (Position.Y + Scale.Y > -coords.Y && Position.Y - Scale.Y < -coords.Y
                                                     && Position.X + Scale.X > coords.X &&
                                                     Position.X - Scale.X < coords.X)
                    if (CanClick)
                    {
                        SoundPlayer.PlayUISound(SoundType.ButtonClick, 1, .5f);
                        Click.Invoke(this, E);
                        UpdateTranslation();
                    }
            }
        }

        public override void OnMouseMove(object Sender, MouseMoveEventArgs E)
        {
            if (Enabled && CanClick)
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(
                    new Vector2(E.Position.X, E.Position.Y),
                    new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                );
                if (Position.Y + Scale.Y > -coords.Y && Position.Y - Scale.Y < -coords.Y
                                                     && Position.X + Scale.X > coords.X &&
                                                     Position.X - Scale.X < coords.X)
                {
                    //Hover?.Invoke(Sender, E);
                    if (!_hasEntered)
                    {
                        HoverEnter?.Invoke();
                        UpdateTranslation();
                        _hasEntered = true;
                    }
                }
                else
                {
                    if (_hasEntered)
                    {
                        HoverExit?.Invoke();
                        UpdateTranslation();
                        _hasEntered = false;
                    }
                }
            }
        }

        public void OnHoverEnter()
        {
            if (Text != null)
            {
                _previousFontColor = Text.TextColor;
                Text.TextColor = new Vector4(0.937f, 0.624f, 0.047f, 1.000f).ToColor();
                Text.UpdateText();
                if (PlaySound)
                    SoundPlayer.PlayUISound(SoundType.ButtonHover, 1f, .2f);
            }

            if (Texture != null)
                if (PlaySound)
                    SoundPlayer.PlayUISound(SoundType.ButtonHover, 1f, .3f);
        }

        public void OnHoverExit()
        {
            if (Text != null)
            {
                Text.TextColor = _previousFontColor;
                Text.UpdateText();
            }
        }

        private void UpdateTranslation()
        {
            _liveTranslation?.UpdateTranslation();
        }
    }
}