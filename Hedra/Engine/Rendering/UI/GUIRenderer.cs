using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using System.Drawing;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GUIRenderer.
	/// </summary>
	public class GUIRenderer : IDisposable
	{
		private readonly List<GUITexture> _textures = new List<GUITexture>();
		private readonly Dictionary<IRenderable, bool> _renderableUi = new Dictionary<IRenderable, bool>();
		public static GUIShader GuiShader = new GUIShader("Shaders/GUI.vert", "Shaders/GUI.frag");
		public static uint TransparentTexture;
		private static bool _inited;	
		public int TextureCount => _textures.Count;
	    public int RenderableCount => _renderableUi.Count;
        private static VBO<Vector2> _vbo;
	    private static VAO<Vector2> _vao;

	    public int DrawCount { get; private set; }

		public GUIRenderer(){
			_vbo = new VBO<Vector2>(
                new []
                {
                    new Vector2(1,1),
                    new Vector2(-1,1),
                    new Vector2(-1,-1),
                    new Vector2(1,-1),
                    new Vector2(1,1),
                    new Vector2(-1,-1),
                }, 6*8, VertexAttribPointerType.Float);

            _vao = new VAO<Vector2>(_vbo);

		    var bmp = new Bitmap(1, 1);
		    bmp.SetPixel(0, 0, Color.FromArgb(0, 0, 0, 0));
		    TransparentTexture = Graphics2D.LoadTexture(bmp);

            DisposeManager.Add(this);
		}

	    public void SetupQuad()
	    {
	        _vao.Bind();
            GL.EnableVertexAttribArray(0);
        }

        public void DrawQuad()
	    {
	        GL.DrawArrays(PrimitiveType.Triangles, 0, _vbo.Count);
        }
		
		public void RescaleTextures(float NewWidth, float NewHeight){
			for(int i = _textures.Count-1; i > -1; i--){
				bool anchorScaleX = false, anchorScaleY = false, anchorPositionX = false, anchorPositionY = false;
				if(_textures[i].Scale.X == 1) anchorScaleX = true;
				if(_textures[i].Scale.Y == 1) anchorScaleY = true;
				if(_textures[i].Position.X == 1) anchorPositionX = true;
				if(_textures[i].Position.Y == 1) anchorPositionY = true;
				
				//Textures[i].Position = new Vector2(Textures[i].Position.X * Constants.WIDTH / NewWidth, Textures[i].Position.Y * Constants.HEIGHT / NewHeight);
				_textures[i].Scale = new Vector2(_textures[i].Scale.X * Constants.WIDTH / NewWidth, _textures[i].Scale.Y * Constants.HEIGHT / NewHeight);
				
				//Textures[i].Position = new Vector2( (AnchorPositionX) ? 1 : Textures[i].Position.X, (AnchorPositionY) ? 1 : Textures[i].Position.Y);
				//_textures[i].Scale = new Vector2( (anchorScaleX) ? 1 : _textures[i].Scale.X, (anchorScaleY) ? 1 : _textures[i].Scale.Y);
			}
		}
		public void Add(GUITexture Texture){
			_textures.Add(Texture);
		}
		public void Add(IRenderable Texture){
			lock(_renderableUi)
				_renderableUi.Add(Texture, true);
		}
		public void Add(IRenderable Texture, bool FirstPass){
			lock(_renderableUi)
				_renderableUi.Add(Texture,FirstPass);
		}		
		public void Remove(GUITexture Texture){
			_textures.Remove(Texture);
		}
		public void Remove(IRenderable Texture){
			lock(_renderableUi)
				_renderableUi.Remove(Texture);
		}
		
		public void Draw(){
			DrawCount = 0;
			Dictionary<IRenderable, bool> tempDic;
			lock(_renderableUi)
				tempDic= new Dictionary<IRenderable, bool>(_renderableUi);
			foreach(KeyValuePair<IRenderable, bool> keyPair  in tempDic){
				if(keyPair.Value){
					keyPair.Key.Draw();
				}
			}
			
			GuiShader.Bind();
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.DepthTest);

            GUITexture[] texturesArray = _textures.ToArray();
			foreach(GUITexture texture in texturesArray){
				if(texture != null && texture.IsEnabled){
					DrawCount++;
					GL.ActiveTexture(TextureUnit.Texture1);
					GL.BindTexture(TextureTarget.Texture2D, texture.BackGroundId);
					GL.Uniform1(GuiShader.BackGroundUniform,1);
						
					GL.ActiveTexture(TextureUnit.Texture0);
					GL.BindTexture(TextureTarget.Texture2D, texture.Id);
					GL.Uniform1(GuiShader.GUIUniform,0);
						
					Vector2 scale = texture.Scale;
				    this.SetupQuad();

                    GL.Uniform2(GuiShader.ScaleUniform, scale);
                    GL.Uniform2(GuiShader.SizeUniform, new Vector2(1.0f / Constants.WIDTH, 1.0f / Constants.HEIGHT));
                    GL.Uniform1(GuiShader.FxaaUniform, texture.Fxaa ? 1.0f : 0.0f);
                    GL.Uniform2(GuiShader.PositionUniform, texture.Position);
					GL.Uniform4(GuiShader.ColorUniform, texture.Color);
					GL.Uniform1(GuiShader.FlippedUniform, texture.IdPointer == null && !texture.Flipped ? 0 : 1);
					GL.Uniform1(GuiShader.OpacityUniform, texture.Opacity);
					GL.Uniform1(GuiShader.GrayscaleUniform, texture.Grayscale ? 1 : 0);
					GL.Uniform4(GuiShader.TintUniform, texture.Tint);
					
					Matrix3 rot = (texture.Angle == 0) ? Matrix3.Identity : texture.RotationMatrix;
					GL.UniformMatrix3(GuiShader.RotationUniform, false, ref rot);
					
                    this.DrawQuad();
				}
			}
			
			GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.Texture2D);
			GL.Enable(EnableCap.CullFace);
			GuiShader.UnBind();
			
			foreach(KeyValuePair<IRenderable, bool> keyPair in tempDic){
				if(!keyPair.Value){
					keyPair.Key.Draw();
				}
			}
		}
		
		public void Draw(GUITexture Texture){
		    if (!Texture.IsEnabled) return;

		    GuiShader.Bind();
		    GL.Enable(EnableCap.Texture2D);
		    GL.Enable(EnableCap.Blend);
		    GL.Disable(EnableCap.DepthTest);

            this.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture1);
		    GL.BindTexture(TextureTarget.Texture2D, Texture.BackGroundId);
		    GL.Uniform1(GuiShader.BackGroundUniform, 1);

		    GL.ActiveTexture(TextureUnit.Texture0);
		    GL.BindTexture(TextureTarget.Texture2D, Texture.Id);
		    GL.Uniform1(GuiShader.GUIUniform, 0);

		    Vector2 scale = Texture.Scale;

		    GL.Uniform2(GuiShader.ScaleUniform, scale);
		    GL.Uniform2(GuiShader.PositionUniform, Texture.Position);
		    GL.Uniform4(GuiShader.ColorUniform, Texture.Color);
		    GL.Uniform1(GuiShader.FlippedUniform, (Texture.IdPointer == null && !Texture.Flipped) ? 0 : 1);
		    GL.Uniform1(GuiShader.OpacityUniform, Texture.Opacity);
		    GL.Uniform1(GuiShader.GrayscaleUniform, (Texture.Grayscale) ? 1 : 0);
		    GL.Uniform4(GuiShader.TintUniform, Texture.Tint);
		    GL.Uniform2(GuiShader.SizeUniform, new Vector2(1.0f / Constants.WIDTH, 1.0f / Constants.HEIGHT));
		    GL.Uniform1(GuiShader.FxaaUniform, (Texture.Fxaa) ? 1.0f : 0.0f);

		    Matrix3 rot = (Texture.Angle == 0) ? Matrix3.Identity : Texture.RotationMatrix;
		    GL.UniformMatrix3(GuiShader.RotationUniform, false, ref rot);
		    this.DrawQuad();

		    GL.Enable(EnableCap.DepthTest);
		    GL.Disable(EnableCap.Blend);
		    GL.Disable(EnableCap.Texture2D);
		    GL.Enable(EnableCap.CullFace);
		    GuiShader.UnBind();
        }
		
		public void Dispose(){
			GL.DeleteBuffers(1, ref _vbo.ID);
		}
	}
}
