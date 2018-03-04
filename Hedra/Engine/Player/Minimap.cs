/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/02/2017
 * Time: 03:29 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using Hedra.Engine.Generation;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Minimap.
	/// </summary>
	public class Minimap : IRenderable
	{
		private LocalPlayer Player;
		private Panel InPanel = new Panel();
		private Texture MiniMap;
		private RenderableTexture MapCursor, MiniMapRing, MiniMapN, MiniMapQ, MiniMapM;
		private FBO MapFBO;
		private MinimapShader Shader = new MinimapShader("Shaders/GUI.vert","Shaders/MinimapGUI.frag");
		private Vector3 LerpPosition;
		
		public Minimap(LocalPlayer Player){
			this.Player = Player;
			MapFBO = new FBO(Constants.WIDTH, Constants.HEIGHT);
			
			MiniMap = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMap.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * new Vector2(1f, 1f));

            DrawManager.UIRenderer.Remove(MiniMap.TextureElement);
			DrawManager.UIRenderer.Add(this, false);
			
			MapCursor = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MapCursor.png"), new Vector2(.8f, .7f), new Vector2(0.008f, 0.019f) * 1.3f), false);
			MiniMapRing = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapRing.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), false);
			MiniMapN = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapNorth.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), false);
			MiniMapQ = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapQuest.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), false);
			MiniMapM = new RenderableTexture(new Texture(Graphics2D.LoadFromAssets("Assets/UI/MiniMapMerchant.png"), new Vector2(.8f, .7f), new Vector2(0.13f, 0.23f) * 1.0f), false);
			
			InPanel.AddElement(MapCursor);
			InPanel.AddElement(MiniMap);
			InPanel.AddElement(MiniMapRing);
			InPanel.AddElement(MiniMapN);
			InPanel.AddElement(MiniMapQ);
			InPanel.AddElement(MiniMapM);
			InPanel.Disable();
		}
		
		public void Update(){
			if (!Show) return;
		}
		
		public void DrawMap(){			
			GL.Enable(EnableCap.DepthTest);
			int PrevShader = GraphicsLayer.ShaderBound;
			int PrevFBO = GraphicsLayer.FboBound;
			LerpPosition = Mathf.Lerp(Player.Model.Position, LerpPosition, (float) Time.deltaTime * 8f);
			MapFBO.Bind();
				GL.ClearColor(Color.FromArgb(0,0,0,0));
				GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
				
				float OldDistance = Player.View.TargetDistance;
				float OldPitch = Player.View.Pitch;
		        float OldFacing = Player.View.Yaw;
		        float OldAddonDistance = Player.View.AddonDistance;

                Player.View.Yaw = -1.55f;
				Player.View.TargetDistance = 1f;
				Player.View.Pitch = -10f;
		        Player.View.AddonDistance = 20f;
                Player.View.RebuildMatrix(Player.Model.Position + Player.Map.Offset + Vector3.UnitY * 48f);
				DrawManager.FrustumObject.SetFrustum(Player.View.Matrix);
				
				GL.MatrixMode(MatrixMode.Projection);				
				Matrix4 ProjMatrix = Matrix4.CreateOrthographic(60, 60, 1, 128);
	            GL.LoadMatrix(ref ProjMatrix);
				
				GL.MatrixMode(MatrixMode.Modelview);

		        Player.Map.DrawMesh(1f,
		        -Player.Model.Model.Rotation.Y * Vector3.UnitY,
		        Player.Model.Position + Vector3.UnitY * 80f, Vector3.Zero);//(Player.Map.LastUpdated - Player.Model.Position) / Map.MapPieceWidth);
				
				Player.View.TargetDistance = OldDistance;
				Player.View.Pitch = OldPitch;
				Player.View.Yaw = OldFacing;
		        Player.View.AddonDistance = OldAddonDistance;

                Player.View.RebuildMatrix();
				DrawManager.FrustumObject.SetFrustum(Player.View.Matrix);
				
			GL.Disable(EnableCap.DepthTest);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, PrevFBO);
			GraphicsLayer.FboBound = PrevFBO;
			GL.UseProgram(PrevShader);
			GraphicsLayer.ShaderBound = PrevShader;
			GL.Enable(EnableCap.Blend);
		}

		public void Draw(){
			if(!GameSettings.ShowMinimap){
				MapCursor.Disable();
				MiniMapRing.Disable();
				MiniMapN.Disable();
				MiniMapQ.Disable();
				MiniMapM.Disable();
				return;
			}
			
			if(!MiniMap.TextureElement.IsEnabled) return;
			MapCursor.Enable();
			MiniMapRing.Enable();
			MiniMapN.Enable();
			//MiniMapQ.Enable();
			if(World.StructureGenerator.MerchantSpawned)
				MiniMapM.Enable();
			else
				MiniMapM.Disable();
			MiniMapN.BaseTexture.TextureElement.Angle = Player.Model.Model.Rotation.Y;
			
			bool Inverted = false;
			Vector3 ToObjDirection = (Player.Model.Model.Position - World.QuestManager.Quest.IconPosition).NormalizedFast().Xz.ToVector3();
			Vector3 Rot = Physics.DirectionToEuler( ToObjDirection );
			MiniMapQ.BaseTexture.TextureElement.Angle =  Player.Model.Model.Rotation.Y - Rot.Y + 180;
			
			
			ToObjDirection = (Player.Model.Model.Position - World.StructureGenerator.MerchantPosition).NormalizedFast().Xz.ToVector3();
			if(ToObjDirection.X < 0 || ToObjDirection.Z < 0){
				ToObjDirection = new Vector3(Math.Abs(ToObjDirection.X), 0, Math.Abs(ToObjDirection.Z));
				Inverted = true;
			}
			Rot = Physics.DirectionToEuler( ToObjDirection );
			MiniMapM.BaseTexture.TextureElement.Angle =  Player.Model.Model.Rotation.Y - Rot.Y + 180 + ((Inverted) ? -180 : 0);
			
			this.DrawMap();
			
			Shader.Bind();
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.DepthTest);

		    DrawManager.UIRenderer.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, MapFBO.TextureID[0]);
			GL.Uniform1(Shader.FillUniform,1);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, MiniMap.TextureElement.TextureId);
			GL.Uniform1(Shader.GuiUniform,0);
						
			GL.Uniform2(Shader.ScaleUniform, MiniMap.TextureElement.Scale);
			GL.Uniform2(Shader.PositionUniform, MiniMap.TextureElement.Position);
			GL.Uniform2(Shader.SizeUniform, MiniMap.TextureElement.Scale * new Vector2(Constants.WIDTH, Constants.HEIGHT) );
			GL.Uniform4(Shader.ColorUniform, MiniMap.TextureElement.Color);
			GL.Uniform1(Shader.FlippedUniform, (MiniMap.TextureElement.IdPointer == null && !MiniMap.TextureElement.Flipped) ? 0 : 1);
			GL.Uniform1(Shader.OpacityUniform, MiniMap.TextureElement.Opacity);
			GL.Uniform1(Shader.GrayscaleUniform, (MiniMap.TextureElement.Grayscale) ? 1 : 0);
			GL.Uniform4(Shader.TintUniform, MiniMap.TextureElement.Tint);

		    DrawManager.UIRenderer.DrawQuad();

            GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.Texture2D);
			GL.Enable(EnableCap.CullFace);
			Shader.UnBind();
		}
		
		private bool m_Show;
		public bool Show{
			get{ return m_Show; }
			set{
				m_Show = value;
				if(m_Show)
					InPanel.Enable();
				else
					InPanel.Disable();
			}
		}
	}
}