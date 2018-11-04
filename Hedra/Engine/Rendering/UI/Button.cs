/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 12:54 a.m.
 *
 */

using System;
using System.Drawing;
using System.IO;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    public delegate void OnButtonClickEventHandler(object Sender, MouseButtonEventArgs E);

    public delegate void OnButtonHoverEventHandler(object Sender, MouseEventArgs E);

    public delegate void OnButtonHoverEnterEventHandler(object Sender, MouseEventArgs E);

    public delegate void OnButtonHoverExitEventHandler(object Sender, MouseEventArgs E);

    public class Button : EventListener, UIElement
    {
        private bool _hasEntered;
        private Vector2 _position;
        private Color _previousFontColor;
        private Vector2 _previousScale;
        private GUIText _privateText;
        private Vector2 _scale;

        public bool Clickable = true;
        public bool Enlarge = true;
        public bool PlaySound = true;
        public GUITexture Texture;

        public bool Enabled { get; set; }

        public GUIText Text
        {
            get => this._privateText;
            private set
            {
                this._privateText?.Dispose();
                this._privateText = value;
            }
        }

        public Button(Vector2 Position, Vector2 Scale, string Text, uint Texture, Color FontColor, Font F)
        {
            this.Initialize(Position, Scale, Text, Texture, FontColor, F);
        }

        public Button(Vector2 Position, Vector2 Scale, string Text, uint Texture, Color FontColor)
        {
            this.Initialize(Position, Scale, Text, Texture, FontColor, SystemFonts.DefaultFont);
        }

        public Button(Vector2 Position, Vector2 Scale, string Text, uint Texture)
        {
            this.Initialize(Position, Scale, Text, Texture, Color.Black, SystemFonts.DefaultFont);
        }

        public Button(Vector2 Position, Vector2 Scale, uint Texture)
        {
            this.Initialize(Position, Scale, null, Texture, Color.Black, SystemFonts.DefaultFont);
        }

        public event OnButtonClickEventHandler Click;
        public event OnButtonHoverEventHandler Hover;
        public event OnButtonHoverEnterEventHandler HoverEnter;
        public event OnButtonHoverExitEventHandler HoverExit;

        private void Initialize(Vector2 Position, Vector2 Scale, string Text, uint TextureId, Color FontColor, Font F)
        {
            if (TextureId != 0)
                this.Texture = new GUITexture(TextureId, Scale, Position);
            if (this.Texture != null)
                DrawManager.UIRenderer.Add(this.Texture);

            if (!string.IsNullOrEmpty(Text))
                this.Text = new GUIText(Text, Position, FontColor, F);

            if (!string.IsNullOrEmpty(Text))
            {
                this.Scale = this.Text.Scale;
            }
            else
            {
                this.Scale = Texture?.Scale ?? Scale;
            }
            this.Position = new Vector2(Position.X, Position.Y);

            this.HoverEnter += this.OnHoverEnter;
            this.HoverExit += this.OnHoverExit;
        }

        public void ForceClick()
        {
            this.Click?.Invoke(null, null);
        }

        public override void OnMouseButtonDown(object Sender, MouseButtonEventArgs E)
        {
            if (this.Enabled && this.Click != null && (E.Button == MouseButton.Left || E.Button == MouseButton.Right))
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(
                    new Vector2(E.Mouse.X, E.Mouse.Y),
                    new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                );
                if (this.Position.Y + this.Scale.Y > -coords.Y && this.Position.Y - this.Scale.Y < -coords.Y
                    && this.Position.X + this.Scale.X > coords.X && this.Position.X - this.Scale.X < coords.X)
                    if (this.Clickable)
                    {
                        SoundManager.PlayUISound(SoundType.ButtonClick, 1, .5f);
                        this.Click.Invoke(Sender, E);
                    }
            }
        }

        public override void OnMouseMove(object Sender, MouseMoveEventArgs E)
        {
            if (this.Enabled && this.Clickable)
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(
                    new Vector2(E.Mouse.X, E.Mouse.Y),
                    new Vector2(GameSettings.SurfaceWidth, GameSettings.SurfaceHeight)
                    );
                if (this.Position.Y + this.Scale.Y > -coords.Y && this.Position.Y - this.Scale.Y < -coords.Y
                    && this.Position.X + this.Scale.X > coords.X && this.Position.X - this.Scale.X < coords.X)
                {
                    Hover?.Invoke(Sender, E);
                    if (!this._hasEntered)
                    {
                        HoverEnter?.Invoke(Sender, E);
                        this._hasEntered = true;
                    }
                }
                else
                {
                    if (this._hasEntered)
                    {
                        HoverExit?.Invoke(Sender, E);
                        this._hasEntered = false;
                    }
                }
            }
        }

        public void OnHoverEnter(object Sender, EventArgs E)
        {
            if (this.Text != null)
            {
                this._previousFontColor = this.Text.TextColor;
                this.Text.TextColor = new Vector4(0.937f, 0.624f, 0.047f, 1.000f).ToColor();
                this.Text.UpdateText();
                if (this.PlaySound)
                    SoundManager.PlayUISound(SoundType.ButtonHover, 1f, .2f);
            }
            if (this.Texture != null)
                if (this.PlaySound)
                    SoundManager.PlayUISound(SoundType.ButtonHover, 1f, .3f);
        }

        public void OnHoverExit(object Sender, EventArgs E)
        {
            if (this.Text != null)
            {
                this.Text.TextColor = this._previousFontColor;
                this.Text.UpdateText();
            }
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
            get => this._scale;
            set
            {
                this._scale = value;
                if (this.Text != null)
                    this.Text.Scale = value;
                if (this.Texture != null)
                    this.Texture.Scale = value;
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