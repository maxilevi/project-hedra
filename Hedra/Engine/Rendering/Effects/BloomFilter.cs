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
using Hedra.Engine.Enviroment;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of BloomFilter.
	/// </summary>
	public class BloomFilter : Filter
	{
		private static GUIShader Bloom =  new GUIShader("Shaders/Bloom.vert", "Shaders/Bloom.frag");
		private static BlurShader HBlurShader = new BlurShader("Shaders/HBlur.vert","Shaders/Blur.frag");
		private static BlurShader VBlurShader = new BlurShader("Shaders/VBlur.vert","Shaders/Blur.frag");
		public FBO HBloomFBO = new FBO(Constants.WIDTH / 4, Constants.HEIGHT / 4);
		public FBO VBloomFBO = new FBO(Constants.WIDTH / 4, Constants.HEIGHT / 4);
		public int TopColorUniform, BotColorUniform, HeightUniform;
		
		public BloomFilter() : base(){
			TopColorUniform = GL.GetUniformLocation(Bloom.ShaderID, "TopColor");
			BotColorUniform = GL.GetUniformLocation(Bloom.ShaderID, "BotColor");
			HeightUniform = GL.GetUniformLocation(Bloom.ShaderID, "Height");
		}
		
		public override void Resize(){
			HBloomFBO = HBloomFBO.Resize();
			VBloomFBO = VBloomFBO.Resize();
		}
		
		public override void Pass(FBO Src, FBO Dst){
			HBloomFBO.Bind();
			Bloom.Bind();
			
			GL.Uniform4(TopColorUniform, SkyManager.Skydome.TopColor);
			GL.Uniform4(BotColorUniform, SkyManager.Skydome.BotColor);
			GL.Uniform1(HeightUniform, Constants.HEIGHT);

            GL.Clear(ClearBufferMask.ColorBufferBit);
			DrawQuad(Src.TextureID[0]);
			
			Bloom.UnBind();
			HBloomFBO.UnBind();
			
			//hblur
			VBloomFBO.Bind();
			HBlurShader.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad(HBloomFBO.TextureID[0]);
			
			HBlurShader.UnBind();
			VBloomFBO.UnBind();
			
			//vblur
			HBloomFBO.Bind();
			VBlurShader.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad(VBloomFBO.TextureID[0]);
			
			VBlurShader.UnBind();
			HBloomFBO.UnBind();
			
			Dst.Bind();
			MainFBO.Shader.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawQuad(HBloomFBO.TextureID[0], 0);
			MainFBO.Shader.UnBind();
			Dst.UnBind();
		}
	}
}
