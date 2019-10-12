/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 18/03/2017
 * Time: 09:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenToolkit.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// IRenderable wrapper
    /// </summary>
    public class RenderableTexture : IRenderable, UIElement, IAdjustable
    {
        public BackgroundTexture BaseTexture { get; set; }
        
        public RenderableTexture(BackgroundTexture BaseTexture, DrawOrder Order)
        {
            this.BaseTexture = BaseTexture;        
            DrawManager.UIRenderer.Remove(BaseTexture.TextureElement);
            DrawManager.UIRenderer.Add(this, Order);
        }
        
        public void Draw()    
        {
            if (!BaseTexture.Enabled || BaseTexture.TextureElement.IdPointer == null && BaseTexture.TextureElement.TextureId == GUIRenderer.TransparentTexture) return;
            DrawManager.UIRenderer.Draw(BaseTexture.TextureElement);
        }

        public void Adjust()
        {
            BaseTexture.TextureElement.Adjust();
        }
        
        public void Enable()
        {
            BaseTexture.Enable();
        }
        
        public void Disable()
        {
            BaseTexture.Disable();
        }
        
        public Vector2 Position
        {
            get => this.BaseTexture.Position;
            set => this.BaseTexture.Position = value;
        }
        
        public Vector2 Scale{
            get => this.BaseTexture.Scale;
            set => this.BaseTexture.Scale = value;
        }

        public void Dispose()
        {
            BaseTexture?.Dispose();

        }
    }
}
