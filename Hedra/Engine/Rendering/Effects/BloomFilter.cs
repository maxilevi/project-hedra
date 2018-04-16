/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EnvironmentSystem;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of BloomFilter.
	/// </summary>
	public class BloomFilter : Filter
	{
	    private static readonly Shader Bloom;
	    private static readonly Shader HBlurShader;
	    private static readonly Shader VBlurShader;
		public FBO HBloomFbo = new FBO(GameSettings.Width / 4, GameSettings.Height / 4);
		public FBO VBloomFbo = new FBO(GameSettings.Width / 4, GameSettings.Height / 4);
		public int TopColorUniform, BotColorUniform, HeightUniform;

	    static BloomFilter()
	    {
	        Bloom = Shader.Build("Shaders/Bloom.vert", "Shaders/Bloom.frag");
	        HBlurShader = Shader.Build("Shaders/HBlur.vert", "Shaders/Blur.frag");
	        VBlurShader = Shader.Build("Shaders/VBlur.vert", "Shaders/Blur.frag");
        }

		public BloomFilter() : base(){
            TopColorUniform = GL.GetUniformLocation(Bloom.ShaderId, "TopColor");
			BotColorUniform = GL.GetUniformLocation(Bloom.ShaderId, "BotColor");
			HeightUniform = GL.GetUniformLocation(Bloom.ShaderId, "Height");
		}
		
		public override void Resize(){
			HBloomFbo = HBloomFbo.Resize();
			VBloomFbo = VBloomFbo.Resize();
		}
		
		public override void Pass(FBO Src, FBO Dst){
			HBloomFbo.Bind();
			Bloom.Bind();
			
			GL.Uniform4(TopColorUniform, SkyManager.Skydome.TopColor);
			GL.Uniform4(BotColorUniform, SkyManager.Skydome.BotColor);
			GL.Uniform1(HeightUniform, GameSettings.Height);

            GL.Clear(ClearBufferMask.ColorBufferBit);
		    this.DrawQuad(Src.TextureID[0]);
			
			Bloom.UnBind();
			HBloomFbo.UnBind();
			
			//hblur
			VBloomFbo.Bind();
			HBlurShader.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
		    this.DrawQuad(HBloomFbo.TextureID[0]);
			
			HBlurShader.UnBind();
			VBloomFbo.UnBind();
			
			//vblur
			HBloomFbo.Bind();
			VBlurShader.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad(VBloomFbo.TextureID[0]);
			
			VBlurShader.UnBind();
			HBloomFbo.UnBind();
			
			Dst.Bind();
			MainFBO.Shader.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);
		    this.DrawQuad(HBloomFbo.TextureID[0], 0);
			MainFBO.Shader.UnBind();
			Dst.UnBind();
		}
	}
}
