﻿/*
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
		
		public override void Resize(){
			HBloomFbo = HBloomFbo.Resize();
			VBloomFbo = VBloomFbo.Resize();
		}
		
		public override void Pass(FBO Src, FBO Dst){
			HBloomFbo.Bind();
			Bloom.Bind();
		    Bloom["Modifier"] = GameSettings.BloomModifier;
            GL.Clear(ClearBufferMask.ColorBufferBit);
		    this.DrawQuad(Bloom, Src.TextureID[0]);
			
			Bloom.Unbind();
			HBloomFbo.UnBind();
			
			VBloomFbo.Bind();
			HBlurShader.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
		    this.DrawQuad(HBlurShader, HBloomFbo.TextureID[0]);
			
			HBlurShader.Unbind();
			VBloomFbo.UnBind();
			
			HBloomFbo.Bind();
			VBlurShader.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            this.DrawQuad(VBlurShader, VBloomFbo.TextureID[0]);
			
			VBlurShader.Unbind();
			HBloomFbo.UnBind();
			
			Dst.Bind();
			MainFBO.Shader.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);
		    this.DrawQuad(MainFBO.Shader, HBloomFbo.TextureID[0], 0);
			MainFBO.Shader.Unbind();
			Dst.UnBind();
		}
	}
}
