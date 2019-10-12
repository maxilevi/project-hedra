using System;
using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Rendering.UI;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Rendering.UI
{
    public class Plaque : UIElement, ITransparent
    {
        private readonly BackgroundTexture _plaqueBackground;
        private readonly GUIText _plaqueText;
        
        public Plaque(Vector2 Position)
        {
            _plaqueBackground = new BackgroundTexture("Assets/UI/Plaque.png", Position, Vector2.One * .45f);
            _plaqueText = new GUIText(
                string.Empty,
                _plaqueBackground.Position,
                Color.White,
                FontCache.GetNormal(11)
             );
        }
        
        
        public string Text
        {
            get => _plaqueText.Text;
            set
            {
                _plaqueText.Text = value;
                var scalar = Math.Min(
                    Math.Min(1, _plaqueBackground.Scale.X * 0.95f / _plaqueText.Scale.X),
                    Math.Min(1, _plaqueBackground.Scale.Y * 0.95f / _plaqueText.Scale.Y)
                );
                _plaqueText.Position = _plaqueBackground.Position;
                _plaqueText.Scale *= scalar;
            }
        }
        
        public float Opacity
        {
            get => _plaqueBackground.TextureElement.Opacity;
            set => _plaqueBackground.TextureElement.Opacity = _plaqueText.UIText.Opacity = value;
        }

        public void Enable()
        {
            _plaqueBackground.Enable();
            _plaqueText.Enable();
        }

        public void Disable()
        {
            _plaqueBackground.Disable();
            _plaqueText.Disable();
        }

        public Vector2 Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public Vector2 Scale
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void Dispose()
        {
            _plaqueBackground.Dispose();
            _plaqueText.Dispose();
        }
    }
}