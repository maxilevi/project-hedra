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
	    public bool Enabled { get; private set; }

        public Texture(string AssetPath, Vector2 Position, Vector2 Scale)
	    {
	        this.TextureElement = new GUITexture(Graphics2D.LoadFromAssets(AssetPath),
	            Graphics2D.SizeFromAssets(AssetPath) * Scale, Position) {Enabled = true};
	        DrawManager.UIRenderer.Add(this.TextureElement);
	    }

        public Texture(uint TextureId, Vector2 Position, Vector2 Scale)
        {
			this.TextureElement = new GUITexture(TextureId, Scale, Position);
			this.TextureElement.Enabled = true;
			DrawManager.UIRenderer.Add(this.TextureElement);
		}
		public Texture(Vector4 TextureColor, Vector2 Position, Vector2 Scale)
        {
			this.TextureElement = new GUITexture(Graphics2D.ColorTexture(TextureColor), Scale, Position);
			this.TextureElement.Enabled = true;
			DrawManager.UIRenderer.Add(this.TextureElement);
		}
		
		public Texture(Color GradientColor0, Color GradientColor1, Vector2 Position, Vector2 Scale, GradientType Type){
			Bitmap bmp = new Bitmap( (int) (Scale.X * GameSettings.Width+1), (int) (Scale.Y*GameSettings.Height+1));
		    bmp = Graphics2D.CreateGradient( GradientColor0, GradientColor1, Type, bmp);
			this.TextureElement = new GUITexture(Graphics2D.LoadTexture(bmp), Scale, Position);
			this.TextureElement.Enabled = true;
			DrawManager.UIRenderer.Add(this.TextureElement);
		}
		
		public Vector2 Scale{
			get { return TextureElement.Scale; }
			set { 
				this.TextureElement.Scale = value;
			}
		}
		
		public Vector2 Position{
			get { return TextureElement.Position; }
			set {
				this.TextureElement.Position = value;
			}
		}

		public void Enable(){
			TextureElement.Enabled = true;
			Enabled = true;			
		}

		public void Disable(){
			TextureElement.Enabled = false;	
			Enabled = false;
		}
		
		public void Dispose(){
			DrawManager.UIRenderer.Remove(this.TextureElement);
		}
	}
}
