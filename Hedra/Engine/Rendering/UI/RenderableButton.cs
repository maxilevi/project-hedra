/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 09:53 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.Sound;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Rendering.UI
{
	
	public class RenderableButton : Button, IRenderable
	{
		public DrawPriority Priority { get; set; }

	    public RenderableButton(Vector2 Position, Vector2 Scale, string Text, uint Texture, Color FontColor, Font F) :
	        base(Position, Scale, Text, Texture, FontColor, F)
	    {
	        DrawManager.UIRenderer.Remove(this.Texture);
        }

	    public RenderableButton(Vector2 Position, Vector2 Scale, string Text, uint Texture, Color FontColor) : base(
	        Position, Scale, Text, Texture, FontColor)
	    {
	        DrawManager.UIRenderer.Remove(this.Texture);
        }
	    public RenderableButton(Vector2 Position, Vector2 Scale, string Text, uint Texture) : base(Position, Scale, Text,
	        Texture)
	    {
	        DrawManager.UIRenderer.Remove(this.Texture);
        }
	    public RenderableButton(Vector2 Position, Vector2 Scale, uint Texture) : base(Position, Scale, Texture)
	    {
	        DrawManager.UIRenderer.Remove(this.Texture);
	    }
		
		public void Draw(){
			if(this.Texture != null)			
				DrawManager.UIRenderer.Draw(this.Texture);
		}
		
		~RenderableButton(){
			ThreadManager.ExecuteOnMainThread( ()=> base.Dispose() );
		}
	}
}
