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
using Hedra.Engine.Management;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    internal delegate void OnButtonClickEventHandler(object Sender, MouseButtonEventArgs E);

    internal delegate void OnButtonHoverEventHandler(object Sender, MouseEventArgs E);

    internal delegate void OnButtonHoverEnterEventHandler(object Sender, MouseEventArgs E);

    internal delegate void OnButtonHoverExitEventHandler(object Sender, MouseEventArgs E);

    internal class Button : EventListener, UIElement, IDisposable
    {
        private bool _hasEntered;
        private Vector2 _position;
        private Color _previousFontColor;
        private Vector2 _previousScale;
        private GUIText _privateText;
        private Vector2 _scale;

        public RectangleF Bounds;
        public bool Clickable = true;
        public bool Enlarge = true;
        public bool PlaySound = true;
        public GUITexture Texture;

        public bool Enabled { get; set; }

        public GUIText Text
        {
            get { return this._privateText; }
            set
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
                this.Bounds = new RectangleF(this.Text.Position.X, this.Text.Position.Y, this.Text.Scale.X,
                    this.Text.Scale.Y);
                this.Scale = this.Text.Scale;
            }
            else
            {
                if (this.Texture != null)
                {
                    var sizeInPixels =
                        Mathf.FromNormalizedDeviceCoordinates(this.Texture.Scale.X, this.Texture.Scale.Y);
                    this.Bounds = new RectangleF(this.Texture.Position.X, this.Texture.Position.Y, Scale.X, Scale.Y);
                    this.Scale = this.Texture.Scale;
                }
                else
                {
                    this.Bounds = new RectangleF(this.Position.X, this.Position.Y, this.Scale.X, this.Scale.Y);
                    this.Scale = Scale;
                }
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
                var coords = Mathf.ToNormalizedDeviceCoordinates(E.Mouse.X, E.Mouse.Y);

                if (this.Position.Y + this.Scale.Y > -coords.Y && this.Position.Y - this.Scale.Y < -coords.Y
                    && this.Position.X + this.Scale.X > coords.X && this.Position.X - this.Scale.X < coords.X)
                    if (this.Clickable)
                    {
                        SoundManager.PlayUISound(SoundType.ButtonClick, 1, 1.5f);
                        this.Click.Invoke(Sender, E);
                    }
            }
        }

        public override void OnMouseMove(object Sender, MouseMoveEventArgs E)
        {
            if (this.Enabled && this.Clickable)
            {
                var coords = Mathf.ToNormalizedDeviceCoordinates(E.Mouse.X, E.Mouse.Y);
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
                var prevPosition = this.Text.Position;
                this.Text.Update();
                this.Text.Position = prevPosition;
                /*PreviousScale = Text.Scale;
                if(Enlarge)
                    Text.Scale = PreviousScale  + PreviousScale / 16;*/
                if (this.PlaySound)
                    SoundManager.PlayUISound(SoundType.OnOff, 1f, .2f);
            }
            if (this.Texture != null)
                if (this.PlaySound)
                    SoundManager.PlayUISound(SoundType.OnOff, 1f, .3f);
        }

        public void OnHoverExit(object Sender, EventArgs E)
        {
            if (this.Text != null)
            {
                this.Text.TextColor = this._previousFontColor;
                var prevPosition = this.Text.Position;
                this.Text.Update();
                this.Text.Position = prevPosition;
                //	Text.Scale = PreviousScale;
            }
            if (this.Texture != null)
            {
                //	Texture.Scale = PreviousScale;
            }
        }

        ~Button()
        {
            ThreadManager.ExecuteOnMainThread(() => this.Dispose());
        }

        public void Dispose()
        {
            if (this.Text != null)
                this.Text.Dispose();
            if (this.Texture != null)
            {
                this.Texture.Dispose();
                DrawManager.UIRenderer.Remove(this.Texture);
            }
        }

        public void Disable()
        {
            this.Enabled = false;
            if (this.Text != null)
                this.Text.Disable();
            if (this.Texture != null)
                this.Texture.Enabled = false;
        }

        public void Enable()
        {
            this.Enabled = true;
            if (this.Text != null)
                this.Text.Enable();
            if (this.Texture != null)
                this.Texture.Enabled = true;
        }

        public Vector2 Position
        {
            get { return this._position; }
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
            get { return this._scale; }
            set
            {
                this._scale = value;
                if (this.Text != null)
                    this.Text.Scale = value;
                if (this.Texture != null)
                    this.Texture.Scale = value;
            }
        }
    }
}