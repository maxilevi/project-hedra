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
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Wrapper around GUIText.
	/// </summary>
	public class RenderableText : IRenderable, IDisposable, UIElement
	{
		public GUIText UIText;

		public RenderableText(string Text, Vector2 Position, Color FontColor, Font FontType){
			this.UIText = new GUIText(Text, Position, FontColor, FontType);
			DrawManager.UIRenderer.Remove(this.UIText.UIText);
		}

        public void Draw(){
			DrawManager.UIRenderer.Draw(UIText.UIText);
		}
		
		public string Text{
			get{ return UIText.Text;}
			set{
				UIText.Text = value;
				DrawManager.UIRenderer.Remove(this.UIText.UIText);
			}
		}
		
		public Vector2 Scale{
			get{ return UIText.Scale;}
			set{
				UIText.Scale = value;
			}
		}
		
		public Vector2 Position{
			get{ return UIText.Position;}
			set{
				UIText.Position = value;
			}
		}
		
		public Color Color{
			get{ return UIText.TextColor; }
			set{ UIText.TextColor = value;}
		}
		
		public void Enable(){
			UIText.Enable();
		}
		
		public void Disable(){
			UIText.Disable();
		}
		
		public void Dispose(){
			UIText.Dispose();
			DrawManager.UIRenderer.Remove(this);
		}
	}
}
