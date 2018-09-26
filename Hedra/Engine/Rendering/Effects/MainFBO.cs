/*
 * Author: Zaphyk
 * Date: 22/02/2016
 * Time: 12:43 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.Management;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Rendering.UI;
using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Do Nothing.
	/// </summary>
	public class MainFBO
	{
	    public static Shader Shader { get; }
        public bool Enabled {get; set;}
		public FBO Default;
	    public FBO ThirdPass;
	    public FBO FinalFbo;
		public FBO AdditiveFbo;
		public UnderWaterFilter UnderWater;
		public DistortionFilter Distortion;
		public BloomFilter Bloom;
		public BlurFilter Blur;	
		public DeferedRenderer Ssao;

	    static MainFBO()
	    {
	        Shader = Shader.Build("Shaders/GUI.vert", "Shaders/GUI.frag");
        }

		public MainFBO()
        {
            Ssao = new DeferedRenderer();
			Default = new FBO(GameSettings.Width, GameSettings.Height, false, 0, FramebufferAttachment.ColorAttachment0, PixelInternalFormat.Rgba32f);
			FinalFbo = new FBO(GameSettings.Width, GameSettings.Height);
			AdditiveFbo = new FBO(GameSettings.Width, GameSettings.Height);

            Bloom = new BloomFilter();
			UnderWater = new UnderWaterFilter();
			Distortion = new DistortionFilter();
			Blur = new BlurFilter();
		}

		public void Draw()
        {
			
			#region Normal
			//Just paste the contents without any effect
			if(!GameSettings.SSAO)
            {
				FinalFbo.Bind();
                Shader.Bind();
				DrawQuad(Default.TextureID[0]);
                Shader.Unbind();
				FinalFbo.UnBind();
				
				if(GameSettings.BlurFilter)
                {
					Default.Bind();
                    Shader.Bind();
					DrawQuad(FinalFbo.TextureID[0]);
                    Shader.Unbind();
					Default.UnBind();
					
					//Clear it
					FinalFbo.Bind();
					Renderer.ClearColor(Colors.Transparent);
					FinalFbo.UnBind();
				}
			}
			#endregion
			
			#region SSAO
			if(GameSettings.SSAO)
			{

			    FBO DrawFBO = (GameSettings.UnderWaterEffect || GameSettings.BlurFilter || GameSettings.DarkEffect) ? Default : FinalFbo;

                Ssao.SecondPass.Bind();
				
				Ssao.FirstPassShader.Bind();
			
                Renderer.Enable(EnableCap.Blend);

                DrawManager.UIRenderer.SetupQuad();

                Renderer.ActiveTexture(TextureUnit.Texture0);
				Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureID[1]);
				
				Renderer.ActiveTexture(TextureUnit.Texture1);
				Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureID[2]);
				
				Renderer.ActiveTexture(TextureUnit.Texture2);
				Renderer.BindTexture(TextureTarget.Texture2D, (uint) Ssao.RandomTex);
				
				Renderer.Uniform1(Ssao.PositionSampler, 0);
				Renderer.Uniform1(Ssao.NormalSampler, 1);
				Renderer.Uniform1(Ssao.RandomSampler, 2);
				Renderer.Uniform1(Ssao.Intensity, GameSettings.AmbientOcclusionIntensity);
				
				Renderer.UniformMatrix4(Ssao.ProjectionUniform, false, ref DrawManager.FrustumObject.ProjectionMatrix);

			    DrawManager.UIRenderer.DrawQuad();

                Ssao.FirstPassShader.Unbind();

			    Ssao.ThirdPass.Bind();
                Ssao.SecondPassShader.Bind();

			    //Firstpass output
			    Renderer.ActiveTexture(TextureUnit.Texture0);
			    Renderer.BindTexture(TextureTarget.Texture2D, Ssao.SecondPass.TextureID[0]);

			    DrawManager.UIRenderer.DrawQuad();

                Ssao.ThirdPass.UnBind();
			    DrawFBO.Bind();
                Ssao.ThirdPassShader.Bind();

                //Firstpass output
                Renderer.ActiveTexture(TextureUnit.Texture0);
				Renderer.BindTexture(TextureTarget.Texture2D, Ssao.ThirdPass.TextureID[0]);
				//Color texture
				Renderer.ActiveTexture(TextureUnit.Texture1);
				Renderer.BindTexture(TextureTarget.Texture2D, Ssao.FirstPass.TextureID[0]);
				
				Renderer.Uniform1( Ssao.AOSampler, 0);
				Renderer.Uniform1( Ssao.ColorSampler, 1);

			    DrawManager.UIRenderer.DrawQuad();

                Ssao.ThirdPassShader.Unbind();
			    DrawFBO.UnBind();//Unbind is the same
				
				Renderer.Enable(EnableCap.CullFace);
				Renderer.Disable(EnableCap.Blend);
			}
			#endregion
			
			#region Bloom
			if(GameSettings.Bloom){
				Bloom.Pass(FinalFbo, AdditiveFbo);
			}
			#endregion
			
			#region UnderWater
			if(GameSettings.UnderWaterEffect)
			{
			    var underChunk = World.GetChunkAt(LocalPlayer.Instance.View.CameraPosition);
			    UnderWater.Multiplier = underChunk?.Biome?.Colors.WaterColor * .7f + Vector4.One * .3f ?? Colors.DeepSkyBlue;
				UnderWater.Pass(Default, FinalFbo);
			}
			#endregion
			
			#region Distortion
			if(GameSettings.DistortEffect){
			//	Distortion.Pass(Default, FinalFBO);
			}
			#endregion
			
			#region Dark
			if(GameSettings.DarkEffect){
				UnderWater.Multiplier = new Vector4(.5f,.5f,.5f,1);
				UnderWater.Pass(Default, FinalFbo);
			}
			#endregion
			
			#region Blur
			if(GameSettings.BlurFilter){
				Blur.Pass(Default, FinalFbo);
			}
			#endregion 
			
			#region UnderWater & SSAO Flip
			if( (GameSettings.UnderWaterEffect || GameSettings.DarkEffect || GameSettings.BlurFilter) && GameSettings.SSAO){
                Default.Bind();
				Shader.Bind();
				DrawQuad(FinalFbo.TextureID[0], 0, true);
                Shader.Unbind();
                Default.UnBind();
				   
				FinalFbo.Bind();
                Shader.Bind();
				DrawQuad(Default.TextureID[0], 0, false);
                Shader.Unbind();
				FinalFbo.UnBind();
                
                //Clear it
                Default.Bind();
				Renderer.ClearColor(Colors.Transparent);
                Default.UnBind();
			}
            #endregion

			Shader.Bind();
			DrawQuad(FinalFbo.TextureID[0], GameSettings.Bloom ? AdditiveFbo.TextureID[0] : 0, false, GameSettings.FXAA);
			Shader.Unbind();
		}
		
		public static void DrawQuad(uint TexID, uint Additive = 0, bool Flipped = false, bool FXAA = false)
        {
			Renderer.Disable(EnableCap.DepthTest);
			
			DrawManager.UIRenderer.SetupQuad();

            Renderer.ActiveTexture(TextureUnit.Texture0);
			Renderer.BindTexture(TextureTarget.Texture2D, TexID);
		    Shader["Texture"] = 0;

            Renderer.ActiveTexture(TextureUnit.Texture1);
			Renderer.BindTexture(TextureTarget.Texture2D, Additive);
		    Shader["Background"] = 1;

            Shader["Scale"] = Vector2.One;
		    Shader["Position"] = Vector2.Zero;
		    Shader["Flipped"] = Flipped ? 1 : 0;
		    Shader["Size"] = new Vector2(1.0f / GameSettings.Width, 1.0f / GameSettings.Height);
		    Shader["FXAA"] = FXAA ? 1.0f : 0.0f;

            DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
		}
		
		public void Clear(){}
		
		public void CaptureData(){
			if(!GameSettings.SSAO)
				Default.Bind();
			else
				Ssao.FirstPass.Bind();
			
			
		}
		public void UnCaptureData(){
			if(!GameSettings.SSAO)
				Default.UnBind();
			else
				Ssao.FirstPass.UnBind();//Unbind ids the same
		}
		
		public static MainFBO DefaultBuffer => DrawManager.MainBuffer;

	    public void Resize()
        {
			Default = Default.Resize();
			FinalFbo = FinalFbo.Resize();
			AdditiveFbo = AdditiveFbo.Resize();
			Ssao.FirstPass = Ssao.FirstPass.Resize();
			Ssao.SecondPass = Ssao.SecondPass.Resize();
	        Ssao.ThirdPass = Ssao.ThirdPass.Resize();
			Bloom.Resize();
			Distortion.Resize();
			Blur.Resize();
			UnderWater.Resize();
		}

	    public void Dispose()
	    {
	        Default.Dispose();
	        AdditiveFbo.Dispose();
            FinalFbo.Dispose();
            Ssao.Dispose();
	        Default.Dispose();
	        Bloom.Dispose();
	        Blur.Dispose();
	        UnderWater.Dispose();
	        Distortion.Dispose();
        }
	}
}
