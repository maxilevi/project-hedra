/*
 * Author: Zaphyk
 * Date: 22/02/2016
 * Time: 12:43 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Rendering.UI;
using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Do Nothing.
	/// </summary>
	public class MainFBO : IEffect
	{
		public bool Enabled{get; set;}
		public FBO Default, ThirdPass;
		public FBO FinalFBO;
		public FBO AdditiveFBO;
		public UnderWaterFilter UnderWater;
		public DistortionFilter Distortion;
		public BloomFilter Bloom;
		public BlurFilter Blur;
		
		public DeferedRenderer SSAO;
        public static GUIShader Shader = new GUIShader("Shaders/GUI.vert", "Shaders/GUI.frag");
		private Matrix4 Identity;
		public MainFBO(){
			Identity = Mathf.CreateTransformationMatrix(new Vector2(1,-1), Vector2.Zero);
			SSAO = new DeferedRenderer();
			Default = new FBO(GameSettings.Width, GameSettings.Height, false, 0, FramebufferAttachment.ColorAttachment0, PixelInternalFormat.Rgba32f);
			FinalFBO = new FBO(GameSettings.Width, GameSettings.Height);
			AdditiveFBO = new FBO(GameSettings.Width, GameSettings.Height);

            Bloom = new BloomFilter();
			UnderWater = new UnderWaterFilter();
			Distortion = new DistortionFilter();
			Blur = new BlurFilter();
		}
		public void Draw(){
			
			#region Normal
			//Just paste the contents without any effect
			if(!GameSettings.UnderWaterEffect && !GameSettings.SSAO){
				FinalFBO.Bind();
                Shader.Bind();
				DrawQuad(Default.TextureID[0]);
                Shader.UnBind();
				FinalFBO.UnBind();
				
				if(GameSettings.BlurFilter){
					Default.Bind();
                    Shader.Bind();
					DrawQuad(FinalFBO.TextureID[0]);
                    Shader.UnBind();
					Default.UnBind();
					
					//Clear it
					FinalFBO.Bind();
					GL.ClearColor(Color.Transparent);
					FinalFBO.UnBind();
				}
			}
			#endregion
			
			#region SSAO
			if(GameSettings.SSAO)
			{

			    FBO DrawFBO = (GameSettings.UnderWaterEffect || GameSettings.BlurFilter || GameSettings.DarkEffect) ? Default : FinalFBO;

                SSAO.SecondPass.Bind();
				
				SSAO.FirstPassShader.Bind();
			
				GL.Enable(EnableCap.Texture2D);
				GL.Enable(EnableCap.Blend);

			    DrawManager.UIRenderer.SetupQuad();

                GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, SSAO.FirstPass.TextureID[1]);
				
				GL.ActiveTexture(TextureUnit.Texture1);
				GL.BindTexture(TextureTarget.Texture2D, SSAO.FirstPass.TextureID[2]);
				
				GL.ActiveTexture(TextureUnit.Texture2);
				GL.BindTexture(TextureTarget.Texture2D, SSAO.RandomTex);
				
				GL.Uniform1(SSAO.PositionSampler, 0);
				GL.Uniform1(SSAO.NormalSampler, 1);
				GL.Uniform1(SSAO.RandomSampler, 2);
				GL.Uniform1(SSAO.Intensity, GameSettings.AmbientOcclusionIntensity);
				
				GL.UniformMatrix4(SSAO.ProjectionUniform, false, ref DrawManager.FrustumObject.ProjectionMatrix);

			    DrawManager.UIRenderer.DrawQuad();

                SSAO.FirstPassShader.UnBind();

			    SSAO.ThirdPass.Bind();
                SSAO.SecondPassShader.Bind();

			    //Firstpass output
			    GL.ActiveTexture(TextureUnit.Texture0);
			    GL.BindTexture(TextureTarget.Texture2D, SSAO.SecondPass.TextureID[0]);

			    DrawManager.UIRenderer.DrawQuad();

                SSAO.ThirdPass.UnBind();
			    DrawFBO.Bind();
                SSAO.ThirdPassShader.Bind();

                //Firstpass output
                GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, SSAO.ThirdPass.TextureID[0]);
				//Color texture
				GL.ActiveTexture(TextureUnit.Texture1);
				GL.BindTexture(TextureTarget.Texture2D, SSAO.FirstPass.TextureID[0]);
				
				GL.Uniform1( SSAO.AOSampler, 0);
				GL.Uniform1( SSAO.ColorSampler, 1);

			    DrawManager.UIRenderer.DrawQuad();

                SSAO.ThirdPassShader.UnBind();
			    DrawFBO.UnBind();//Unbind is the same
				
				GL.Enable(EnableCap.CullFace);
				GL.Disable(EnableCap.Blend);
				GL.Disable(EnableCap.Texture2D);
			}
			#endregion
			
			#region Bloom
			if(GameSettings.Bloom){
				Bloom.Pass(FinalFBO, AdditiveFBO);
			}
			#endregion
			
			#region UnderWater
			if(GameSettings.UnderWaterEffect)
			{
			    var underChunk = World.GetChunkAt(LocalPlayer.Instance.View.Position);
				UnderWaterFilter.Multiplier = underChunk?.Biome?.Colors.WaterColor * 0.8f ?? Colors.DeepSkyBlue;
				UnderWater.Pass(Default, FinalFBO);
			}
			#endregion
			
			#region Distortion
			if(GameSettings.DistortEffect){
			//	Distortion.Pass(Default, FinalFBO);
			}
			#endregion
			
			#region Dark
			if(GameSettings.DarkEffect){
				UnderWaterFilter.Multiplier = new Vector4(.5f,.5f,.5f,1);
				UnderWater.Pass(Default, FinalFBO);
			}
			#endregion
			
			#region Blur
			if(GameSettings.BlurFilter){
				Blur.Pass(Default, FinalFBO);
			}
			#endregion 
			
			#region UnderWater & SSAO Flip
			if( (GameSettings.UnderWaterEffect || GameSettings.DarkEffect || GameSettings.BlurFilter) && GameSettings.SSAO && !GameSettings.Bloom){
                Default.Bind();
				Shader.Bind();
				DrawQuad(FinalFBO.TextureID[0], 0, true);
                Shader.UnBind();
                Default.UnBind();
				   
				FinalFBO.Bind();
                Shader.Bind();
				DrawQuad(Default.TextureID[0], 0, false);
                Shader.UnBind();
				FinalFBO.UnBind();

                //Clear it
                Default.Bind();
				GL.ClearColor(Color.Transparent);
                Default.UnBind();
			}
            #endregion

			Shader.Bind();
			DrawQuad(FinalFBO.TextureID[0], (GameSettings.Bloom) ? AdditiveFBO.TextureID[0] : 0, false, GameSettings.FXAA);
			Shader.UnBind();
		}
		
		public static void DrawQuad(uint TexID, uint Additive = 0, bool Flipped = false, bool FXAA = false){
			GL.Enable(EnableCap.Texture2D);
			GL.Disable(EnableCap.DepthTest);
			
			DrawManager.UIRenderer.SetupQuad();
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexID);
			GL.Uniform1(Shader.GUIUniform, 0);
			
			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, Additive);
			GL.Uniform1(Shader.BackGroundUniform, 1);
			
			GL.Uniform1(Shader.FlippedUniform, (Flipped) ? 1 : 0);
			
			GL.Uniform2(Shader.ScaleUniform, new Vector2(1,1));
			GL.Uniform2(Shader.PositionUniform, new Vector2(0,0));
            GL.Uniform2(Shader.SizeUniform, new Vector2(1.0f/GameSettings.Width, 1.0f/GameSettings.Height));
            GL.Uniform1(Shader.FxaaUniform, (FXAA) ? 1.0f : 0.0f);

		    DrawManager.UIRenderer.DrawQuad();

            GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Texture2D);
		}
		
		public void Clear(){}
		
		public void CaptureData(){
			if(!GameSettings.SSAO)
				Default.Bind();
			else
				SSAO.FirstPass.Bind();
			
			
		}
		public void UnCaptureData(){
			if(!GameSettings.SSAO)
				Default.UnBind();
			else
				SSAO.FirstPass.UnBind();//Unbind ids the same
		}
		
		public static MainFBO DefaultBuffer => DrawManager.MainBuffer;

	    public void Resize(){
			Default = Default.Resize();
			FinalFBO = FinalFBO.Resize();
			AdditiveFBO = AdditiveFBO.Resize();
			SSAO.FirstPass = SSAO.FirstPass.Resize();
			SSAO.SecondPass = SSAO.SecondPass.Resize();
	        SSAO.ThirdPass = SSAO.ThirdPass.Resize();
			Bloom.Resize();
			Distortion.Resize();
			Blur.Resize();
			UnderWater.Resize();
		}
	}
}
