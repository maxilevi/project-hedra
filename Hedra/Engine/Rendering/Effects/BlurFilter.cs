/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/08/2016
 * Time: 01:16 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of BloomFilter.
	/// </summary>
	internal class BlurFilter : Filter
	{
		
		private static readonly Shader HBlurShader;
		private static readonly Shader VBlurShader;

	    static BlurFilter()
	    {
	        HBlurShader = Shader.Build("Shaders/HBlur.vert", "Shaders/Blur.frag");
	        VBlurShader = Shader.Build("Shaders/VBlur.vert", "Shaders/Blur.frag");
	    }

	    public override void Pass(FBO Src, FBO Dst){
			
			Dst.Bind();
			HBlurShader.Bind();
		    this.DrawQuad(HBlurShader, Src.TextureID[0], 0);
			HBlurShader.Unbind();
			Dst.UnBind();

			Src.Bind();
			
			VBlurShader.Bind();
		    this.DrawQuad(VBlurShader, Dst.TextureID[0], 0);
			VBlurShader.Unbind();
			
			Src.UnBind();
			
			Dst.Bind();
			MainFBO.Shader.Bind();
			MainFBO.DrawQuad(Src.TextureID[0]);
			MainFBO.Shader.Unbind();
			Dst.UnBind();
		}
		
		public override void DrawQuad(Shader DrawingShader, uint TexID, uint Additive = 0, bool Flipped = false){

			Renderer.Enable(EnableCap.Texture2D);
			Renderer.Disable(EnableCap.DepthTest);

		    DrawManager.UIRenderer.SetupQuad();

            GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, TexID);

		    DrawManager.UIRenderer.DrawQuad();

            Renderer.Enable(EnableCap.DepthTest);
			Renderer.Enable(EnableCap.CullFace);
			Renderer.Disable(EnableCap.Texture2D);
		}

	    public override void Resize()
	    {
	        
	    }

	    public override void Dispose()
	    {

	    }
    }
}
