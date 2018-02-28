/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of BloomFilter.
	/// </summary>
	public class BlurFilter : Filter
	{
		
		private static BlurShader HBlurShader = new BlurShader("Shaders/HBlur.vert","Shaders/Blur.frag");
		private static BlurShader VBlurShader = new BlurShader("Shaders/VBlur.vert","Shaders/Blur.frag");
		
		public BlurFilter() : base(){}
		
		public override void Resize(){
	
		}
		
		public override void Pass(FBO Src, FBO Dst){
			
			Dst.Bind();
			HBlurShader.Bind();
			DrawQuad(Src.TextureID[0], 0);
			HBlurShader.UnBind();
			Dst.UnBind();

			Src.Bind();
			
			VBlurShader.Bind();
			DrawQuad(Dst.TextureID[0], 0);
			VBlurShader.UnBind();
			
			Src.UnBind();
			
			Dst.Bind();
			MainFBO.Shader.Bind();
			MainFBO.DrawQuad(Src.TextureID[0]);
			MainFBO.Shader.UnBind();
			Dst.UnBind();
		}
		
		public override void DrawQuad(uint TexID, uint Additive = 0, bool Flipped = false){

			GL.Enable(EnableCap.Texture2D);
			GL.Disable(EnableCap.DepthTest);

		    DrawManager.UIRenderer.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexID);

		    DrawManager.UIRenderer.DrawQuad();

            GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.Disable(EnableCap.Texture2D);
		}
	}
}
