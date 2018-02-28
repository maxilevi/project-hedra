/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 27/04/2016
 * Time: 08:26 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Management;
using System.IO;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of Texture.
	/// </summary>
	public class Texture : UIElement, IDisposable
	{
		public static uint Background = Graphics2D.LoadTexture(Graphics2D.ReColorMask(Color.FromArgb(255,69,69,69),new Bitmap(new MemoryStream(AssetManager.ReadBinary("Assets/Background.png", AssetManager.DataFile3)))));
		public GUITexture TextureElement;
		
		public Texture(uint TextureId, Vector2 Position, Vector2 Scale){
			this.TextureElement = new GUITexture(TextureId, Scale, Position);
			this.TextureElement.IsEnabled = true;
			DrawManager.UIRenderer.Add(this.TextureElement);
		}
		public Texture(Vector4 TextureColor, Vector2 Position, Vector2 Scale){
			this.TextureElement = new GUITexture(Graphics2D.ColorTexture(TextureColor), Scale, Position);
			this.TextureElement.IsEnabled = true;
			DrawManager.UIRenderer.Add(this.TextureElement);
		}
		
		public Texture(Color GradientColor0, Color GradientColor1, Vector2 Position, Vector2 Scale, GradientType Type){
			Bitmap bmp = new Bitmap( (int) (Scale.X * Constants.WIDTH+1), (int) (Scale.Y*Constants.HEIGHT+1));
		    bmp = Graphics2D.CreateGradient( GradientColor0, GradientColor1, Type, bmp);
			this.TextureElement = new GUITexture(Graphics2D.LoadTexture(bmp), Scale, Position);
			this.TextureElement.IsEnabled = true;
			DrawManager.UIRenderer.Add(this.TextureElement);
		}
		
		private Vector2 _mScale;
		public Vector2 Scale{
			get { return _mScale; }
			set { 
				this._mScale = value;
				this.TextureElement.Scale = value;
			}
		}
		
		private Vector2 _mPosition;
		public Vector2 Position{
			get { return _mPosition; }
			set {
				_mPosition = value;
				this.TextureElement.Position = value;
			}
		}
		public bool Enabled {get; private set;}
		public void Enable(){
			TextureElement.IsEnabled = true;
			Enabled = true;			
		}
		public void Disable(){
			TextureElement.IsEnabled = false;	
			Enabled = false;
		}
		
		public void Dispose(){
			DrawManager.UIRenderer.Remove(this.TextureElement);
		}
	}
}
