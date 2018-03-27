/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/06/2016
 * Time: 12:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering
{
	//Kind of hacky, refactor later
	public class Billboard : IRenderable, IDisposable
	{
		public Vector3 Position = Vector3.Zero;
		public float LifeTime;
		public bool Disposed;
		public bool Vanish;
		public float Size = 1;
		public float Speed = 2;
		public Func<Vector3> FollowFunc;
		public UIElement Texture;
		public bool Enabled {get; set;}
		
		
		private bool TextBillboard = true;
		private Vector2 OriginalScale;
		
		public Billboard(float LifeTime, string Text, Color TextColor, Font TextFont, Vector3 Position)
		{
			TextBillboard = true;
			this.LifeTime = LifeTime;
			this.Position = Position;
			this.Texture = new GUIText(Text, Vector2.Zero, TextColor, TextFont);
			this.Texture.Enable();
			this.OriginalScale = this.Texture.Scale;
			DrawManager.UIRenderer.Remove( (this.Texture as GUIText).UIText);
			
			DrawManager.Add(this);
		}
		
		public Billboard(float LifeTime, uint Icon, Vector3 Position, Vector2 Scale)
		{
			TextBillboard = false;
			this.LifeTime = LifeTime;
			this.Position = Position;
			this.Texture = new Texture(Icon, Vector2.Zero, Scale);
			this.Texture.Enable();
			this.OriginalScale = Scale;
			DrawManager.UIRenderer.Remove( (this.Texture as Texture).TextureElement);
			
			DrawManager.Add(this);
		}
		
		private float Life = 0;
		private Vector3 AddedPosition;
		public void Draw(){
			if(FollowFunc != null)
				Position = FollowFunc();
			
			if(Vanish){
				AddedPosition += Vector3.UnitY * Time.FrameTimeSeconds * Speed;
				(this.Texture as GUIText).UIText.Opacity = 1-(Life / LifeTime);
			}
			Life += Time.FrameTimeSeconds;
			
			if(LifeTime != 0 && Life >= LifeTime){
				this.Dispose();
				return;
			}
			
			LocalPlayer Player = Scenes.SceneManager.Game.Player;
			float Product = Mathf.DotProduct(Player.View.LookAtPoint.NormalizedFast(), (Position+AddedPosition - Player.Position).NormalizedFast());
			if(Product <= -0.5f) return;
			
			Vector4 EyeSpace = Vector4.Transform(new Vector4(Position+AddedPosition,1), DrawManager.FrustumObject.ModelViewMatrix);
			Vector4 HomogeneusSpace = Vector4.Transform(EyeSpace, DrawManager.FrustumObject.ProjectionMatrix);
			Vector3 NDC = HomogeneusSpace.Xyz / HomogeneusSpace.W;
			this.Texture.Position = Mathf.Clamp(NDC.Xy, -.98f, .98f);
			this.Texture.Scale = this.OriginalScale * Size;
			
			if(TextBillboard)
				DrawManager.UIRenderer.Draw((this.Texture as GUIText).UIText);
			else
				DrawManager.UIRenderer.Draw((this.Texture as Texture).TextureElement);
		}
		
		public void Dispose(){
			DrawManager.Remove(this);
			Disposed = true;
		}
	}
}
