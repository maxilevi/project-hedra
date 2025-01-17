/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/06/2016
 * Time: 04:14 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Game;
using Hedra.Rendering.UI;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Wrapper around GUIText.
    /// </summary>
    public class RenderableText : IRenderable, IAdjustable, UIElement
    {
        public readonly GUIText UIText;

        public RenderableText(string Text, Vector2 Position, Color FontColor, Font FontType)
        {
            UIText = new GUIText(Text, Position, FontColor, FontType);
            DrawManager.UIRenderer.Remove(UIText.UIText);
        }

        public string Text
        {
            get => UIText.Text;
            set
            {
                UIText.Text = value;
                DrawManager.UIRenderer.Remove(UIText.UIText);
            }
        }

        public Color Color
        {
            get => UIText.TextColor;
            set
            {
                UIText.TextColor = value;
                DrawManager.UIRenderer.Remove(UIText.UIText);
            }
        }

        public Font TextFont
        {
            get => UIText.TextFont;
            set
            {
                UIText.TextFont = value;
                DrawManager.UIRenderer.Remove(UIText.UIText);
            }
        }

        public bool Stroke
        {
            get => UIText.Stroke;
            set => UIText.Stroke = value;
        }

        public float StrokeWidth
        {
            get => UIText.StrokeWidth;
            set => UIText.StrokeWidth = value;
        }

        public Color StrokeColor
        {
            get => UIText.StrokeColor;
            set => UIText.StrokeColor = value;
        }

        public bool Enabled => UIText.Enabled;

        public void Adjust()
        {
            UIText.UIText.Adjust();
        }

        public void Draw()
        {
            if (!UIText.UIText.Enabled ||
                UIText.UIText.IdPointer == null && UIText.UIText.Id == GUIRenderer.TransparentTexture) return;
            if ((UIText.UIText.Scale * new Vector2(GameSettings.Width, GameSettings.Height)).LengthSquared() <
                2) return;
            DrawManager.UIRenderer.Draw(UIText.UIText);
        }

        public Vector2 Scale
        {
            get => UIText.Scale;
            set => UIText.Scale = value;
        }

        public Vector2 Position
        {
            get => UIText.Position;
            set => UIText.Position = value;
        }

        public void Enable()
        {
            UIText.Enable();
        }

        public void Disable()
        {
            UIText.Disable();
        }

        public void Dispose()
        {
            UIText.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }
    }
}