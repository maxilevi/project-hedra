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
		public GUIText UiText;

		public RenderableText(string Text, Vector2 Position, Color FontColor, Font F){
			this.UiText = new GUIText(Text, Position, FontColor, F);
			DrawManager.UIRenderer.Remove(this.UiText.UiText);
		}
		
		public void Draw(){
			DrawManager.UIRenderer.Draw(UiText.UiText);
		}
		
		public string Text{
			get{ return UiText.Text;}
			set{
				UiText.Text = value;
				DrawManager.UIRenderer.Remove(this.UiText.UiText);
			}
		}
		
		public Vector2 Scale{
			get{ return UiText.Scale;}
			set{
				UiText.Scale = value;
			}
		}
		
		public Vector2 Position{
			get{ return UiText.Position;}
			set{
				UiText.Position = value;
			}
		}
		
		public Vector4 Color{
			get{ return UiText.Color; }
			set{ UiText.Color = value;}
		}
		
		public void Enable(){
			UiText.Enable();
		}
		
		public void Disable(){
			UiText.Disable();
		}
		
		public void Dispose(){
			UiText.Dispose();
			DrawManager.UIRenderer.Remove(this);
		}
	}
}
