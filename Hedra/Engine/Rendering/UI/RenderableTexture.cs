/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 18/03/2017
 * Time: 09:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// IRenderable wrapper
	/// </summary>
	public class RenderableTexture : IRenderable, UIElement
	{
		public Texture BaseTexture { get; set; }
		
		public RenderableTexture(Texture BaseTexture, DrawOrder Order)
        {
			this.BaseTexture = BaseTexture;		
			DrawManager.UIRenderer.Remove(BaseTexture.TextureElement);
			DrawManager.UIRenderer.Add(this, Order);
		}
		
		public void Draw()
        {
			DrawManager.UIRenderer.Draw(BaseTexture.TextureElement);
		}
		
		public void Enable()
        {
			BaseTexture.Enable();
		}
		
		public void Disable()
        {
			BaseTexture.Disable();
		}

	    public DrawOrder Order
	    {
	        get => DrawManager.UIRenderer.GetDrawOrder(this);
	        set => DrawManager.UIRenderer.SetDrawOrder(this, value);
	    }
		
		public Vector2 Position{
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
