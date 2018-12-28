/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/06/2016
 * Time: 04:14 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Drawing;
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Wrapper around GUIText.
    /// </summary>
    public class RenderableText : IRenderable, IAdjustable, UIElement
    {
        public readonly GUIText UIText;

        public RenderableText(string Text, Vector2 Position, Color FontColor, Font FontType){
            this.UIText = new GUIText(Text, Position, FontColor, FontType);
            DrawManager.UIRenderer.Remove(this.UIText.UIText);
        }

        public void Draw()
        {
            if (!UIText.UIText.Enabled) return;
            DrawManager.UIRenderer.Draw(UIText.UIText);
        }

        public void Adjust()
        {
            UIText.UIText.Adjust();
        }
        
        public string Text
        {
            get => UIText.Text;
            set{
                UIText.Text = value;
                DrawManager.UIRenderer.Remove(this.UIText.UIText);
            }
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
        
        public Color Color{
            get => UIText.TextColor;
            set => UIText.TextColor = value;
        }

        public Font TextFont
        {
            get => UIText.TextFont;
            set => UIText.TextFont = value;
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
