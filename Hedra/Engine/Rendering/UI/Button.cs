/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 12:54 a.m.
 *
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Rendering.UI;
using Hedra.Sound;
using System.Numerics;
using MouseButton = Silk.NET.Input.Common.MouseButton;

namespace Hedra.Engine.Rendering.UI
{
    public delegate void OnButtonClickEventHandler(object Sender, MouseButtonEventArgs E);
    

    public delegate void OnButtonHoverEnterEventHandler();

    public delegate void OnButtonHoverExitEventHandler();

    public class Button : EventListener, UIElement
    {
        private bool _hasEntered;
        private Vector2 _position;
        private Color _previousFontColor;
        private Vector2 _previousScale;
        private GUIText _privateText;
        private Translation _liveTranslation;

        public bool CanClick { get; set; } = true;
        public bool PlaySound { get; set; } = true;
        public GUITexture Texture { get; set; }

        public bool Enabled { get; private set; }

        public GUIText Text
        {
            get => this._privateText;
            private set
            {
                this._privateText?.Dispose();
                this._privateText = value;
            }
        }

        public Button(Vector2 Position, Vector2 Scale, Translation Translation, Color FontColor, Font TextFont)
        {
            this.Initialize(Position, Scale, Translation.Get(), Translation, 0, FontColor, TextFont);
        }
        
        public Button(Vector2 Position, Vector2 Scale, string Text, Color FontColor, Font TextFont)
        {
            this.Initialize(Position, Scale, Text, null, 0, FontColor, TextFont);
        }

        public Button(Vector2 Position, Vector2 Scale, string Text, uint Texture, Color FontColor)
        {
            this.Initialize(Position, Scale, Text, null, Texture, FontColor, SystemFonts.DefaultFont);
        }

        public Button(Vector2 Position, Vector2 Scale, string Text, uint Texture)
        {
            this.Initialize(Position, Scale, Text, null, Texture, Color.Black, SystemFonts.DefaultFont);
        }

        public Button(Vector2 Position, Vector2 Scale, uint Texture)
        {
            this.Initialize(Position, Scale, null, null, Texture, Color.Black, SystemFonts.DefaultFont);
        }

        public event OnButtonClickEventHandler Click;
        public event OnButtonHoverEnterEventHandler HoverEnter;
        public event OnButtonHoverExitEventHandler HoverExit;

        private void Initialize(Vector2 Position, Vector2 Scale, string Text, Translation Translation, uint TextureId, Color FontColor, Font F)
        {
            if (TextureId != 0)
                this.Texture = new GUITexture(TextureId, Scale, Position);
            if (this.Texture != null)
                DrawManager.UIRenderer.Add(this.Texture);

            if (!string.IsNullOrEmpty(Text) || Translation != null)
            {
                _liveTranslation = Translation ?? Translation.Default(Text);
                this.Text = new GUIText(_liveTranslation, Position, FontColor, F);
            }
            this.Position = new Vector2(Position.X, Position.Y);

            this.HoverEnter += this.OnHoverEnter;
            this.HoverExit += this.OnHoverExit;
        }

        public void ForceClick()
        {
            this.Click?.Invoke(null, default);
        }

        public override void OnMouseButtonDown(object Sender, MouseButtonEventArgs E)
        {
            if (this.Enabled && this.Click != null && (E.Button == MouseButton.Left || E.Button == MouseButton.Right))
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(
                    new Vector2(E.Position.X, E.Position.Y),
                    new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                );
                if (this.Position.Y + this.Scale.Y > -coords.Y && this.Position.Y - this.Scale.Y < -coords.Y
                    && this.Position.X + this.Scale.X > coords.X && this.Position.X - this.Scale.X < coords.X)
                {
                    if (this.CanClick)
                    {
                        SoundPlayer.PlayUISound(SoundType.ButtonClick, 1, .5f);
                        this.Click.Invoke(this, E);
                        UpdateTranslation();
                    }
                }
            }
        }

        public override void OnMouseMove(object Sender, MouseMoveEventArgs E)
        {
            if (this.Enabled && this.CanClick)
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(
                    new Vector2(E.Position.X, E.Position.Y),
                    new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                );
                if (this.Position.Y + this.Scale.Y > -coords.Y && this.Position.Y - this.Scale.Y < -coords.Y
                    && this.Position.X + this.Scale.X > coords.X && this.Position.X - this.Scale.X < coords.X)
                {
                    //Hover?.Invoke(Sender, E);
                    if (!this._hasEntered)
                    {
                        HoverEnter?.Invoke();
                        UpdateTranslation();
                        this._hasEntered = true;
                    }
                }
                else
                {
                    if (this._hasEntered)
                    {
                        HoverExit?.Invoke();
                        UpdateTranslation();
                        this._hasEntered = false;
                    }
                }
            }
        }

        public void OnHoverEnter()
        {
            if (this.Text != null)
            {
                this._previousFontColor = this.Text.TextColor;
                this.Text.TextColor = new Vector4(0.937f, 0.624f, 0.047f, 1.000f).ToColor();
                this.Text.UpdateText();
                if (this.PlaySound)
                    SoundPlayer.PlayUISound(SoundType.ButtonHover, 1f, .2f);
            }
            if (this.Texture != null)
                if (this.PlaySound)
                    SoundPlayer.PlayUISound(SoundType.ButtonHover, 1f, .3f);
        }

        public void OnHoverExit()
        {
            if (this.Text != null)
            {
                this.Text.TextColor = this._previousFontColor;
                this.Text.UpdateText();
            }
        }

        private void UpdateTranslation()
        {
            _liveTranslation?.UpdateTranslation();
        }

        public void Disable()
        {
            this.Enabled = false;
            Text?.Disable();
            if (this.Texture != null)
                this.Texture.Enabled = false;
        }

        public void Enable()
        {
            this.Enabled = true;
            Text?.Enable();
            if (this.Texture != null)
                this.Texture.Enabled = true;
        }

        public Vector2 Position
        {
            get => this._position;
            set
            {
                this._position = value;
                if (this.Text != null)
                    this.Text.Position = value;
                if (this.Texture != null)
                    this.Texture.Position = value;
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
            if (this.Texture != null)
            {
                this.Texture.Dispose();
                DrawManager.UIRenderer.Remove(this.Texture);
            }
        }
                    
        ~Button()
        {
            Executer.ExecuteOnMainThread(this.Dispose);
        }
    }
}